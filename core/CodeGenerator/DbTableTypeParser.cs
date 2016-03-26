using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeGenerator
{
    public class DbTableTypeParser
    {
        public List<DbTableTypeDeclaration> Parse(string sql)
        {
            var decls = new List<DbTableTypeDeclaration>();

            var text = sql.Replace('\n', ' ').Replace('\r', ' ');
            var matches = Regex.Matches(text,
                                        @"CREATE\s+TYPE\s+(\[dbo\]\.)?\[(\w+)\]\s?AS\s?TABLE\s?\((.+?)[^0-9]\)");
            foreach (Match match in matches)
            {
                var decl = new DbTableTypeDeclaration();
                decl.TypeName = match.Groups[2].Value;
                decl.Fields = new List<DbField>();

                var paramsText = match.Groups[3].Value.Trim();
                if (paramsText.StartsWith("(") && paramsText.EndsWith(")"))
                    paramsText = paramsText.Substring(1, paramsText.Length - 2);

                foreach (var p in paramsText.Split(','))
                {
                    var fields = p.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Where(w => w.ToLower() != "as").ToArray();
                    if (fields.Length == 0)
                        continue;

                    var fieldName = fields[0];
                    var fieldType = fields[1];

                    var type = DbTypeHelper.GetTypeFromSqlType(fieldType);
                    if (type == null)
                        throw new Exception("Cannot resolve type: " + fieldType);

                    decl.Fields.Add(new DbField
                    {
                        Name = fieldName.Replace("[", "").Replace("]", ""),
                        Type = type.Item1,
                        Len = type.Item2,
                        Dir = ""
                    });
                }
                decls.Add(decl);
            }

            return decls;
        }
    }
}