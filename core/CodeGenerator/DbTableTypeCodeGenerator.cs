using System.Linq;
using CodeWriter;

namespace CodeGenerator
{
    public class DbTableTypeCodeGenerator
    {
        public void Generate(DbTableTypeDeclaration d, CodeWriter.CodeWriter w)
        {
            using (w.B($"public class {d.ClassName}"))
            {
                w._($"public DataTable Table {{ get; set; }}");
                w._();

                using (w.B($"public {d.ClassName}()"))
                {
                    w._($"Table = new DataTable();");
                    foreach (var field in d.Fields)
                    {
                        w._($"Table.Columns.Add(`{field.Name}`, typeof({field.Type}));");
                    }
                }

                var funcDecl = string.Join(", ", d.Fields.Select(f => f.Type + " " + f.Name));
                var funcParams = string.Join(", ", d.Fields.Select(f => f.Name));
                using (w.B($"public void Add({funcDecl})"))
                {
                    w._($"Table.Rows.Add({funcParams});");
                }
            }
        }
    }
}
