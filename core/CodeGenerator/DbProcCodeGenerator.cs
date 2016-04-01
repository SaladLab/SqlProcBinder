using System;
using System.Linq;
using CodeWriter;

namespace CodeGenerator
{
    public class DbProcCodeGenerator
    {
        public void Generate(DbProcDeclaration d, CodeWriter.CodeWriter w)
        {
            using (w.B($"public class {d.ClassName}"))
            {
                if (NeedResultStruct(d))
                {
                    GenerateResultStruct(d, w);
                    w._();
                }

                GenerateExecuteMethod(d, w);
            }
        }

        public bool NeedResultStruct(DbProcDeclaration d)
        {
            return d.Return ||
                   string.IsNullOrEmpty(d.Rowset) == false ||
                   d.Params.Exists(p => p.IsOutput);
        }

        public void GenerateResultStruct(DbProcDeclaration d, CodeWriter.CodeWriter w)
        {
            using (w.B($"public struct Result"))
            {
                if (string.IsNullOrEmpty(d.Rowset))
                    w._($"public int AffectedRowCount;");
                else if (d.RowsetFetch)
                    w._($"public List<{d.Rowset}.Row> Rows;");
                else
                    w._($"public {d.Rowset} Rowset;");

                if (d.Return)
                    w._($"public int Return;");
                foreach (var p in d.Params.Where(p => p.IsOutput))
                    w._($"public {DbTypeHelper.GetMemberDecl(p)};");
            }
        }

        public void GenerateExecuteMethod(DbProcDeclaration d, CodeWriter.CodeWriter w)
        {
            var returnType = NeedResultStruct(d) ? "Result" : "int";
            var paramStr = string.Concat(d.Params.Where(p => p.IsInput)
                                          .Select(p => ", " + DbTypeHelper.GetParamDecl(p)));

            using (w.B($"public static async Task<{returnType}> ExecuteAsync(IDbContext dc{paramStr})"))
            {
                w._($"var ctx = dc.CreateCommand();",
                    $"var cmd = ctx.Command;",
                    $"cmd.CommandType = CommandType.StoredProcedure;",
                    $"cmd.CommandText = `{d.ProcName}`;");

                var pidx = 0;
                foreach (var p in d.Params)
                {
                    var lenStr = (p.Len != 0) ? (", " + p.Len) : "";
                    if (p.IsInput && p.IsOutput == false)
                    {
                        if (p.Type == "DataTable")
                        {
                            EnsureUsingSqlClient(w);
                            w._($"((SqlCommand)cmd).Parameters.AddWithValue(`@{p.Name}`, {p.Name})" +
                                $".SqlDbType = SqlDbType.Structured;");
                        }
                        else
                        {
                            w._($"cmd.AddParameter(`@{p.Name}`, {p.Name});");
                        }
                    }
                    else if (p.IsInput && p.IsOutput)
                    {
                        w._($"var p{pidx} = cmd.AddParameter(`@{p.Name}`, {p.Name}, " +
                            $"ParameterDirection.InputOutput{lenStr});");
                    }
                    else
                    {
                        var ivalue = DbTypeHelper.GetInitValue(p.Type);
                        w._($"var p{pidx} = cmd.AddParameter(`@{p.Name}`, {ivalue}, " +
                            $"ParameterDirection.Output{lenStr});");
                    }
                    pidx += 1;
                }
                if (d.Return)
                {
                    w._($"var pr = cmd.AddParameter(null, 0, ParameterDirection.ReturnValue);");
                }

                w._($"ctx.OnExecuting();");

                if (NeedResultStruct(d))
                {
                    if (string.IsNullOrEmpty(d.Rowset))
                        w._($"var rowCount = await cmd.ExecuteNonQueryAsync();");
                    else
                        w._($"var reader = await cmd.ExecuteReaderAsync();");

                    w._($"var r = new Result();");

                    if (string.IsNullOrEmpty(d.Rowset))
                        w._($"r.AffectedRowCount = rowCount;");
                    else if (d.RowsetFetch)
                        w._($"r.Rows = await (new {d.Rowset}(reader)).FetchAllRowsAndDisposeAsync();");
                    else if (d.Rowset == "DbDataReader")
                        w._($"r.Rowset = reader;");
                    else
                        w._($"r.Rowset = new {d.Rowset}(reader);");

                    pidx = 0;
                    foreach (var p in d.Params)
                    {
                        if (p.IsOutput)
                        {
                            if (p.Nullable)
                            {
                                var ntype = DbTypeHelper.GetType(p);
                                w._($"r.{p.Name} = (p{pidx}.Value is DBNull) ? ({ntype})null : ({p.Type})p{pidx}.Value;");
                            }
                            else
                            {
                                var ivalue = DbTypeHelper.GetInitValue(p.Type);
                                w._($"r.{p.Name} = (p{pidx}.Value is DBNull) ? {ivalue} : ({p.Type})p{pidx}.Value;");
                            }
                        }
                        pidx += 1;
                    }

                    if (d.Return)
                        w._($"r.Return = (int)pr.Value;");
                }
                else
                {
                    w._($"var r = await cmd.ExecuteNonQueryAsync();");
                }

                w._($"ctx.OnExecuted();");
                w._($"return r;");
            }
        }

        private void EnsureUsingSqlClient(CodeWriter.CodeWriter w)
        {
            var line = "using System.Data.SqlClient;";
            if (w.HeadLines.Contains(line))
                return;

            var idx = Array.FindLastIndex(w.HeadLines, l => l.StartsWith("using System.Data."));

            var lineList = w.HeadLines.ToList();
            lineList.Insert(idx + 1, line);
            w.HeadLines = lineList.ToArray();
        }
    }
}
