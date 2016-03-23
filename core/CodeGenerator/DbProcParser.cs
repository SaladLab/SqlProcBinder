using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

                foreach (var p in match.Groups[3].Value.Split(','))
                {
                    var parameters = p.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Where(w => w.ToLower() != "as").ToArray();
                    if (parameters.Length == 0)
                        continue;

                    var paramName = parameters[0];
                    var paramType = parameters[1];
                    var paramOutput = (parameters.Length >= 3 && parameters[2].ToLower() == "output");

                    var type = DbHelper.GetTypeFromSqlType(paramType);
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
