using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CodeGenerator
{
    internal class DbSqlModule : ICodeGenModule
    {
#pragma warning disable 0649
        public class Input
        {
            public string Name;
            public string Sql;
            public string Rowset;
            public List<DbHelper.Field> Params;
        }
#pragma warning restore 0649

        public void Generate(JObject input, string inputPath, ICodeGenWriter writer)
        {
            var i = JsonConvert.DeserializeObject<Input>(input.ToString());

            // Class

            var sb = new StringBuilder();
            sb.AppendFormat("public class {0}\n", i.Name);
            sb.AppendLine("{");

            var paramStr = string.Join(", ", i.Params.Select(DbHelper.GetParamDecl));

            sb.AppendFormat("\tpublic static async Task<{0}> ExecuteAsync(SqlProcBinder.IDbContext dc, {1})\n",
                            string.IsNullOrEmpty(i.Rowset) ? "int" : i.Rowset,
                            paramStr);
            sb.AppendLine("\t{");

            var sql = Regex.Replace(i.Sql, @"(\r?\n)(\s)*", " ");
            sb.AppendFormat("\t\tvar ctx = dc.CreateCommand();\n");
            sb.AppendFormat("\t\tvar cmd = (SqlCommand)ctx.Command;\n");
            sb.AppendFormat("\t\tcmd.CommandText = \"{0}\";\n", sql);
            foreach (var p in i.Params)
            {
                sb.AppendFormat("\t\tcmd.Parameters.AddWithValue(\"@{0}\", {1});\n", p.Name, p.Name);
            }

            sb.AppendLine("\t\tctx.OnExecuting();");
            if (string.IsNullOrEmpty(i.Rowset))
            {
                sb.AppendFormat("\t\tvar r = await cmd.ExecuteNonQueryAsync();\n");
            }
            else
            {
                sb.AppendFormat("\t\tvar reader = await cmd.ExecuteReaderAsync();\n");
                sb.AppendFormat("\t\tvar r = new {0}(reader);\n", i.Rowset);
            }

            sb.AppendLine("\t\tctx.OnExecuted();");
            sb.AppendLine("\t\treturn r;");
            sb.AppendLine("\t}");
            sb.Append("}");

            writer.AddCode(sb.ToString());
        }
    }
}
