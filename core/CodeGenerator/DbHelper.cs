using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CodeGenerator
{
    internal static class DbHelper
    {
        public class Field
        {
            public string Name;
            public string Type;
            public int Len;
            public string Dir;

            [JsonIgnore]
            public bool IsInput
            {
                get { return string.IsNullOrEmpty(Dir) || Dir == "in" || Dir == "ref"; }
            }

            [JsonIgnore]
            public bool IsOutput
            {
                get { return Dir == "ref" || Dir == "out"; }
            }
        }

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

        public static string GetBclType(string type)
        {
            switch (type)
            {
                case "bool":
                    return "Boolean";
                case "byte":
                    return "Byte";
                case "short":
                    return "Int16";
                case "int":
                    return "Int32";
                case "long":
                    return "Int64";
                case "float":
                    return "Float";
                case "double":
                    return "Double";
                case "DateTime":
                    return "DateTime";
                case "string":
                    return "String";
                case "Guid":
                    return "Guid";
                default:
                    return type;
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
                    return "(long)0";
                case "float":
                    return "0f";
                case "double":
                    return "0.0";
                case "DateTime":
                    return "DateTime.MinValue";
                case "string":
                    return "string.Empty";
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
                case "DateTime":
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
                case "float(53)":
                    return Tuple.Create("double", 0);
                case "smalldatetime":
                    return Tuple.Create("DateTime", 0);
                case "datetime":
                    return Tuple.Create("DateTime", 0);
                case "text":
                    return Tuple.Create("string", 0);
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
            if (t.StartsWith("utt"))
            {
                return Tuple.Create("DataTable", 0);
            }

            return null;
        }

        public static string GetParamDecl(Field p)
        {
            var deco = GetDirDecoration(p.Dir);
            return (string.IsNullOrEmpty(deco) ? p.Type : deco + " " + p.Type) + " " + p.Name;
        }

        public static string GetMemberDecl(Field p)
        {
            return p.Type + " " + p.Name;
        }
    }
}
