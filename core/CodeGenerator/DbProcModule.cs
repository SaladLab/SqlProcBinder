using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CodeGenerator
{
    internal class DbProcModule : ICodeGenModule
    {
#pragma warning disable 0649
        public class Input
        {
            public string Name;
            public string Proc;
            public string Path;
            public bool Return;
            public string Rowset;
            public List<DbHelper.Field> Params;
        }
#pragma warning restore 0649

        [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP2101:MethodMustNotContainMoreLinesThan",
            Justification = "NotYet")]
        public void Generate(JObject input, string inputPath, ICodeGenWriter writer)
        {
            var i = JsonConvert.DeserializeObject<Input>(input.ToString());

            // Extract parameter information from stored-procedure definitions

            if (!string.IsNullOrEmpty(i.Path))
            {
                var procPath = Path.Combine(Path.GetDirectoryName(inputPath), i.Path);
                var decl = ExtractProcedureDecl(procPath, i.Proc);
                if (decl == null)
                    throw new Exception("Cannot find: " + procPath + " " + i.Proc);
                if (string.IsNullOrEmpty(i.Proc))
                    i.Proc = decl.Item1;
                i.Params = decl.Item2;
            }

            // Class

            var sb = new StringBuilder();
            sb.AppendFormat("public class {0}\n", i.Name);
            sb.AppendLine("{");

            var resultExists = (i.Return ||
                                !string.IsNullOrEmpty(i.Rowset) ||
                                i.Params.Exists(p => p.IsOutput));
            if (resultExists)
            {
                sb.AppendLine("\tpublic struct Result");
                sb.AppendLine("\t{");
                if (string.IsNullOrEmpty(i.Rowset))
                    sb.AppendLine("\t\tpublic int AffectedRowCount;");
                else
                    sb.AppendFormat("\t\tpublic {0} Rowset;\n", i.Rowset);
                if (i.Return)
                    sb.AppendLine("\t\tpublic int Return;");
                foreach (var p in i.Params.Where(p => p.IsOutput))
                    sb.AppendFormat("\t\tpublic {0};\n", DbHelper.GetMemberDecl(p));
                sb.AppendLine("\t}");
                sb.AppendLine("");
            }

            var paramStr = string.Join(", ", i.Params.Where(p => p.IsInput).Select(DbHelper.GetParamDecl));
            sb.AppendFormat("\tpublic static async Task<{0}> ExecuteAsync(SqlProcBinder.IDbContext dc{1}{2})\n",
                            resultExists ? "Result" : "int",
                            paramStr.Length > 0 ? ", " : "",
                            paramStr);
            sb.AppendLine("\t{");

            sb.AppendFormat("\t\tvar ctx = dc.CreateCommand();\n");
            sb.AppendFormat("\t\tvar cmd = (SqlCommand)ctx.Command;\n");
            sb.AppendFormat("\t\tcmd.CommandType = CommandType.StoredProcedure;\n");
            sb.AppendFormat("\t\tcmd.CommandText = \"{0}\";\n", i.Proc);
            var pidx = 0;
            foreach (var p in i.Params)
            {
                if (p.IsInput && p.IsOutput == false)
                {
                    if (p.Type == "DataTable")
                    {
                        sb.AppendFormat(
                            "\t\tcmd.Parameters.AddWithValue(\"@{0}\", {1}).SqlDbType = SqlDbType.Structured;\n",
                            p.Name, p.Name);
                    }
                    else
                    {
                        if (p.Type == "string")
                        {
                            sb.AppendFormat(
                                "\t\tif ({0} != null)\n" +
                                "\t\t\tcmd.Parameters.AddWithValue(\"@{0}\", {1});\n" +
                                "\t\telse\n" +
                                "\t\t\tcmd.Parameters.AddWithValue(\"@{0}\", DBNull.Value);\n",
                                p.Name, p.Name);
                        }
                        else
                        {
                            sb.AppendFormat(
                                "\t\tcmd.Parameters.AddWithValue(\"@{0}\", {1});\n",
                                p.Name, p.Name);
                        }
                    }
                }
                else if (p.IsInput && p.IsOutput)
                {
                    sb.AppendFormat("\t\tcmd.Parameters.AddWithValue(\"@{0}\", {1});\n", p.Name, p.Name);
                    sb.AppendFormat("\t\tp{0}.Direction = ParameterDirection.InputOutput;\n", pidx);
                    if (p.Len > 0)
                        sb.AppendFormat("\t\tp{0}.Size = {1};\n", pidx, p.Len);
                }
                else
                {
                    sb.AppendFormat(
                        "\t\tvar p{0} = cmd.Parameters.AddWithValue(\"@{1}\", {2});\n",
                        pidx, p.Name, DbHelper.GetInitValue(p.Type));
                    sb.AppendFormat(
                        "\t\tp{0}.Direction = ParameterDirection.Output;\n",
                        pidx);

                    if (p.Len != 0)
                        sb.AppendFormat("\t\tp{0}.Size = {1};\n", pidx, p.Len);
                }
                pidx += 1;
            }
            if (i.Return)
            {
                sb.AppendLine("\t\tvar pr = new SqlParameter();");
                sb.AppendLine("\t\tpr.Direction = ParameterDirection.ReturnValue;");
                sb.AppendLine("\t\tcmd.Parameters.Add(pr);");
            }

            sb.AppendLine("\t\tctx.OnExecuting();");
            if (resultExists)
            {
                if (string.IsNullOrEmpty(i.Rowset))
                    sb.AppendLine("\t\tvar rowCount = await cmd.ExecuteNonQueryAsync();");
                else
                    sb.AppendFormat("\t\tvar reader = await cmd.ExecuteReaderAsync();\n");

                sb.AppendLine("\t\tvar r = new Result();");

                if (string.IsNullOrEmpty(i.Rowset))
                    sb.AppendLine("\t\tr.AffectedRowCount = rowCount;");
                else
                    sb.AppendFormat("\t\tr.Rowset = new {0}(reader);\n", i.Rowset);

                pidx = 0;
                foreach (var p in i.Params)
                {
                    if (p.IsOutput)
                    {
                        if (DbHelper.IsValueType(p.Type))
                        {
                            sb.AppendFormat(
                                "\t\tr.{0} = (p{2}.Value is DBNull) ? {3} : ({1})p{2}.Value;\n",
                                p.Name, p.Type, pidx, DbHelper.GetInitValue(p.Type));
                        }
                        else
                        {
                            sb.AppendFormat(
                                "\t\tr.{0} = (p{2}.Value is DBNull) ? null : ({1})p{2}.Value;\n",
                                p.Name, p.Type, pidx);
                        }
                    }
                    pidx += 1;
                }

                if (i.Return)
                    sb.AppendLine("\t\tr.Return = (int)pr.Value;");
            }
            else
            {
                sb.AppendLine("\t\tvar r = await cmd.ExecuteNonQueryAsync();");
            }

            sb.AppendLine("\t\tctx.OnExecuted();");
            sb.AppendLine("\t\treturn r;");
            sb.AppendLine("\t}");

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }

        private static Tuple<string, List<DbHelper.Field>> ExtractProcedureDecl(string path, string procName)
        {
            var text = File.ReadAllText(path).Replace('\n', ' ').Replace('\r', ' ');
            var matches = Regex.Matches(text, @"CREATE PROCEDURE (\[dbo\]\.)?\[(\w+)\](.+?)AS.*?BEGIN");
            foreach (Match match in matches)
            {
                if (!string.IsNullOrEmpty(procName) && procName != match.Groups[2].Value)
                    continue;

                var ps = new List<DbHelper.Field>();
                foreach (var p in match.Groups[3].Value.Split(','))
                {
                    var decls = p.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Where(w => w.ToLower() != "as").ToArray();
                    if (decls.Length == 0)
                        continue;

                    var paramName = decls[0];
                    var paramType = decls[1];
                    var paramOutput = (decls.Length >= 3 && decls[2].ToLower() == "output");

                    var type = DbHelper.GetTypeFromSqlType(paramType);
                    ps.Add(new DbHelper.Field
                    {
                        Name = paramName.Substring(1),
                        Type = type.Item1,
                        Len = type.Item2,
                        Dir = paramOutput ? "out" : ""
                    });
                }
                return new Tuple<string, List<DbHelper.Field>>(match.Groups[2].Value, ps);
            }

            return null;
        }
    }
}
