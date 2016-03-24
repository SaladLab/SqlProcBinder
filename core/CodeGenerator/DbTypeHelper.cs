using System;
using System.Text.RegularExpressions;

namespace CodeGenerator
{
    public static class DbTypeHelper
    {
        public static string GetDirDecoration(string dir)
        {
            switch (dir)
            {
                case "in":
                    return "";
                case "out":
                    return "out";
                case "ref":
                case "inout":
                    return "ref";
                default:
                    return "";
            }
        }

        public static string GetInitValue(string type)
        {
            switch (type)
            {
                case "bool":
                    return "false";
                case "byte":
                    return "(byte)0";
                case "short":
                    return "(short)0";
                case "int":
                    return "0";
                case "long":
                    return "0L";
                case "float":
                    return "0f";
                case "double":
                    return "0.0";
                case "Decimal":
                    return "0M";
                case "DateTime":
                    return "DateTime.MinValue";
                case "DateTimeOffset":
                    return "DateTimeOffset.MinValue";
                case "TimeSpan":
                    return "TimeSpan.Zero";
                case "string":
                    return "string.Empty";
                case "byte[]":
                    return "new byte[0]";
                case "Guid":
                    return "Guid.Empty";
                default:
                    return "0";
            }
        }

        public static bool IsValueType(string type)
        {
            switch (type)
            {
                case "bool":
                case "byte":
                case "short":
                case "int":
                case "long":
                case "float":
                case "double":
                case "Decimal":
                case "DateTime":
                case "DateTimeOffset":
                case "TimeSpan":
                case "Guid":
                    return true;

                case "string":
                    return false;

                default:
                    return false;
            }
        }

        public static Tuple<string, int> GetTypeFromSqlType(string type)
        {
            var t = type.ToLower().Replace("[", "").Replace("]", "");
            switch (t)
            {
                case "bit":
                    return Tuple.Create("bool", 0);
                case "tinyint":
                    return Tuple.Create("byte", 0);
                case "smallint":
                    return Tuple.Create("short", 0);
                case "int":
                    return Tuple.Create("int", 0);
                case "bigint":
                    return Tuple.Create("long", 0);
                case "real":
                    return Tuple.Create("float", 0);
                case "float":
                case "float(53)":
                    return Tuple.Create("double", 0);
                case "money":
                case "decimal":
                    return Tuple.Create("Decimal", 0);
                case "smalldatetime":
                    return Tuple.Create("DateTime", 0);
                case "date":
                case "datetime":
                case "datetime2":
                    return Tuple.Create("DateTime", 0);
                case "datetimeoffset":
                    return Tuple.Create("DateTimeOffset", 0);
                case "time":
                    return Tuple.Create("TimeSpan", 0);
                case "uniqueidentifier":
                    return Tuple.Create("Guid", 0);
            }
            if (t.StartsWith("char") ||
                t.StartsWith("varchar") ||
                t.StartsWith("nchar") ||
                t.StartsWith("nvarchar"))
            {
                var mo = Regex.Match(t, @"\w*\((\w+)\)");
                if (mo.Success)
                {
                    var param = mo.Groups[1].Value;
                    var size = (param.ToLower() == "max") ? -1 : int.Parse(mo.Groups[1].Value);
                    return Tuple.Create("string", size);
                }
            }
            if (t.StartsWith("binary") ||
                t.StartsWith("varbinary"))
            {
                var mo = Regex.Match(t, @"\w*\((\w+)\)");
                if (mo.Success)
                {
                    var param = mo.Groups[1].Value;
                    var size = (param.ToLower() == "max") ? -1 : int.Parse(mo.Groups[1].Value);
                    return Tuple.Create("byte[]", size);
                }
            }

            return null;
        }

        public static string GetParamDecl(DbField p)
        {
            var deco = GetDirDecoration(p.Dir);
            return (string.IsNullOrEmpty(deco) ? p.Type : deco + " " + p.Type) + " " + p.Name;
        }

        public static string GetMemberDecl(DbField p)
        {
            if (p.Nullable && IsValueType(p.Type))
                return p.Type + "? " + p.Name;
            else
                return p.Type + " " + p.Name;
        }
    }
}
