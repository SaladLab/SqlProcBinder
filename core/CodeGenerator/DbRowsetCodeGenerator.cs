using CodeWriter;

namespace CodeGenerator
{
    public class DbRowsetCodeGenerator
    {
        public void Generate(DbRowsetDeclaration d, CodeWriter.CodeWriter w)
        {
            using (w.B($"public class {d.ClassName} : IDisposable"))
            {
                w._($"private DbDataReader _reader;");
                w._();

                using (w.B($"public {d.ClassName}(DbDataReader reader)"))
                {
                    w._($"_reader = reader;");
                }

                using (w.B($"public class Row"))
                {
                    foreach (var field in d.Fields)
                    {
                        w._($"public {DbTypeHelper.GetMemberDecl(field)};");
                    }
                }

                GenerateMethods(d, w);
            }
        }

        public void GenerateMethods(DbRowsetDeclaration d, CodeWriter.CodeWriter w)
        {
            using (w.B($"public async Task<Row> NextAsync()"))
            {
                w._($"if (await _reader.ReadAsync() == false) return null;",
                    $"var r = new Row();");

                var fidx = 0;
                foreach (var field in d.Fields)
                {
                    w._($"var v{fidx} = _reader.GetValue({fidx});");
                    if (field.Nullable)
                    {
                        var ntype = field.Type + (DbTypeHelper.IsValueType(field.Type) ? "?" : "");
                        w._($"r.{field.Name} = (v{fidx} is DBNull) ? ({ntype})null : ({field.Type})v{fidx};");
                    }
                    else
                    {
                        var ivalue = DbTypeHelper.GetInitValue(field.Type);
                        w._($"r.{field.Name} = (v{fidx} is DBNull) ? {ivalue} : ({field.Type})v{fidx};");
                    }
                    fidx += 1;
                }

                w._($"return r;");
            }

            using (w.B($"public async Task<List<Row>> FetchAllRowsAndDisposeAsync()"))
            {
                w._($"var rows = new List<Row>();");
                using (w.b($"while (true)"))
                {
                    w._($"var row = await NextAsync();",
                        $"if (row == null) break;",
                        $"rows.Add(row);");
                }
                w._($"Dispose();",
                    $"return rows;");
            }

            using (w.B($"public async Task<List<T>> FetchAllRowsAndDisposeAsync<T>(Func<Row, T> selector)"))
            {
                w._($"var rows = new List<T>();");
                using (w.b($"while (true)"))
                {
                    w._($"var row = await NextAsync();",
                        $"if (row == null) break;",
                        $"rows.Add(selector(row));");
                }
                w._($"Dispose();",
                    $"return rows;");
            }

            using (w.B($"public void Dispose()"))
            {
                w._($"_reader.Dispose();");
            }
        }
    }
}
