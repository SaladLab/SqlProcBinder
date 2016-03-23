using System.Text;

namespace CodeGenerator
{
    public class DbRowsetCodeGenerator
    {
        public void Generate(DbRowsetDeclaration d, ICodeGenWriter writer)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("public class {0} : IDisposable\n", d.ClassName);
            sb.AppendLine("{");

            sb.AppendLine("\tprivate DbDataReader _reader;");
            sb.AppendLine("");
            sb.AppendFormat("\tpublic {0}(DbDataReader reader)\n", d.ClassName);
            sb.AppendLine("\t{");
            sb.AppendLine("\t\t_reader = reader;");
            sb.AppendLine("\t}");
            sb.AppendLine("");

            sb.AppendLine("\tpublic class Row");
            sb.AppendLine("\t{");
            foreach (var field in d.Fields)
            {
                sb.AppendFormat("\t\tpublic {0};\n", DbHelper.GetMemberDecl(field));
            }
            sb.AppendLine("\t}");
            sb.AppendLine("");

            sb.AppendLine("\tpublic async Task<Row> NextAsync()");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tif (await _reader.ReadAsync() == false) return null;");
            sb.AppendLine("\t\tvar r = new Row();");
            var fidx = 0;
            foreach (var field in d.Fields)
            {
                var bclType = DbHelper.GetBclType(field.Type);
                if (bclType != "Boolean")
                {
                    sb.AppendFormat("\t\tr.{0} = _reader.Get{1}({2});\n",
                                    field.Name, DbHelper.GetBclType(field.Type), fidx);
                }
                else
                {
                    sb.AppendFormat("\t\tr.{0} = _reader.GetByte({1}) != 0;\n",
                                    field.Name, fidx);
                }
                fidx += 1;
            }
            sb.AppendLine("\t\treturn r;");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine("\tpublic async Task<List<T>> FetchAllRowsAndDisposeAsync<T>(Func<Row, T> selector)");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tvar rows = new List<T>();");
            sb.AppendLine("\t\twhile (true)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tvar row = await NextAsync();");
            sb.AppendLine("\t\t\tif (row == null) break;");
            sb.AppendLine("\t\t\trows.Add(selector(row));");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tDispose();");
            sb.AppendLine("\t\treturn rows;");
            sb.AppendLine("\t}");

            sb.AppendLine("");
            sb.AppendLine("\tpublic void Dispose()");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\t_reader.Dispose();");
            sb.AppendLine("\t}");

            sb.Append("}");

            writer.AddCode(sb.ToString());
        }
    }
}
