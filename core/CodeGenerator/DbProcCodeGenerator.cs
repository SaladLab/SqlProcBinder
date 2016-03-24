using System.Linq;
using System.Text;

namespace CodeGenerator
{
    public class DbProcCodeGenerator
    {
        public void Generate(DbProcDeclaration d, ICodeGenWriter writer)
        {
            // Class

            var sb = new StringBuilder();
            sb.AppendFormat("public class {0}\n", d.ClassName);
            sb.AppendLine("{");

            var resultExists = (d.Return ||
                                !string.IsNullOrEmpty(d.Rowset) ||
                                d.Params.Exists(p => p.IsOutput));
            if (resultExists)
            {
                sb.AppendLine("\tpublic struct Result");
                sb.AppendLine("\t{");
                if (string.IsNullOrEmpty(d.Rowset))
                    sb.AppendLine("\t\tpublic int AffectedRowCount;");
                else if (d.RowsetFetch)
                    sb.AppendFormat("\t\tpublic List<{0}.Row> Rows;\n", d.Rowset);
                else
                    sb.AppendFormat("\t\tpublic {0} Rowset;\n", d.Rowset);

                if (d.Return)
                    sb.AppendLine("\t\tpublic int Return;");
                foreach (var p in d.Params.Where(p => p.IsOutput))
                    sb.AppendFormat("\t\tpublic {0};\n", DbTypeHelper.GetMemberDecl(p));
                sb.AppendLine("\t}");
                sb.AppendLine("");
            }

            var paramStr = string.Join(", ", d.Params.Where(p => p.IsInput).Select(DbTypeHelper.GetParamDecl));
            sb.AppendFormat("\tpublic static async Task<{0}> ExecuteAsync(SqlProcBinder.IDbContext dc{1}{2})\n",
                            resultExists ? "Result" : "int",
                            paramStr.Length > 0 ? ", " : "",
                            paramStr);
            sb.AppendLine("\t{");

            sb.AppendFormat("\t\tvar ctx = dc.CreateCommand();\n");
            sb.AppendFormat("\t\tvar cmd = (SqlCommand)ctx.Command;\n");
            sb.AppendFormat("\t\tcmd.CommandType = CommandType.StoredProcedure;\n");
            sb.AppendFormat("\t\tcmd.CommandText = \"{0}\";\n", d.ProcName);
            var pidx = 0;
            foreach (var p in d.Params)
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
                        pidx, p.Name, DbTypeHelper.GetInitValue(p.Type));
                    sb.AppendFormat(
                        "\t\tp{0}.Direction = ParameterDirection.Output;\n",
                        pidx);

                    if (p.Len != 0)
                        sb.AppendFormat("\t\tp{0}.Size = {1};\n", pidx, p.Len);
                }
                pidx += 1;
            }
            if (d.Return)
            {
                sb.AppendLine("\t\tvar pr = new SqlParameter();");
                sb.AppendLine("\t\tpr.Direction = ParameterDirection.ReturnValue;");
                sb.AppendLine("\t\tcmd.Parameters.Add(pr);");
            }

            sb.AppendLine("\t\tctx.OnExecuting();");
            if (resultExists)
            {
                if (string.IsNullOrEmpty(d.Rowset))
                    sb.AppendLine("\t\tvar rowCount = await cmd.ExecuteNonQueryAsync();");
                else
                    sb.AppendFormat("\t\tvar reader = await cmd.ExecuteReaderAsync();\n");

                sb.AppendLine("\t\tvar r = new Result();");

                if (string.IsNullOrEmpty(d.Rowset))
                    sb.AppendLine("\t\tr.AffectedRowCount = rowCount;");
                else if (d.RowsetFetch)
                    sb.AppendFormat("\t\tr.Rows = await (new {0}(reader)).FetchAllRowsAndDisposeAsync();\n", d.Rowset);
                else if (d.Rowset == "DbDataReader")
                    sb.AppendFormat("\t\tr.Rowset = reader;\n", d.Rowset);
                else
                    sb.AppendFormat("\t\tr.Rowset = new {0}(reader);\n", d.Rowset);

                pidx = 0;
                foreach (var p in d.Params)
                {
                    if (p.IsOutput)
                    {
                        if (DbTypeHelper.IsValueType(p.Type))
                        {
                            sb.AppendFormat(
                                "\t\tr.{0} = (p{2}.Value is DBNull) ? {3} : ({1})p{2}.Value;\n",
                                p.Name, p.Type, pidx, DbTypeHelper.GetInitValue(p.Type));
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

                if (d.Return)
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
    }
}
