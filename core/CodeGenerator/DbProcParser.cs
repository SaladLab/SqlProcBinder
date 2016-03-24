using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeGenerator
{
    public class DbProcParser
    {
        public List<DbProcDeclaration> Parse(string sql)
        {
            var decls = new List<DbProcDeclaration>();

            var text = sql.Replace('\n', ' ').Replace('\r', ' ');
            var matches = Regex.Matches(text,
                                        @"CREATE\s+PROCEDURE\s+(\[dbo\]\.)?\[(\w+)\](.+?)AS\s+BEGIN",
                                        RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var decl = new DbProcDeclaration();
                decl.ProcName = match.Groups[2].Value;
                decl.Params = new List<DbHelper.Field>();

                var paramsText = match.Groups[3].Value.Trim();
                if (paramsText.StartsWith("(") && paramsText.EndsWith(")"))
                    paramsText = paramsText.Substring(1, paramsText.Length - 2);

                foreach (var p in paramsText.Split(','))
                {
                    var parameters = p.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Where(w => w.ToLower() != "as").ToArray();
                    if (parameters.Length == 0)
                        continue;

                    var paramName = parameters[0];
                    var paramType = parameters[1];
                    var paramOutput = (parameters.Length >= 3 && parameters[2].ToLower() == "output");

                    var type = DbHelper.GetTypeFromSqlType(paramType);
                    if (type == null)
                        throw new Exception("Cannot resolve type: " + paramType);

                    decl.Params.Add(new DbHelper.Field
                    {
                        Name = paramName.Substring(1),
                        Type = type.Item1,
                        Len = type.Item2,
                        Dir = paramOutput ? "out" : ""
                    });
                }
                decls.Add(decl);
            }

            return decls;
        }
    }
}
