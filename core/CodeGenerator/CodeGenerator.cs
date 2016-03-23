using System;
using Newtonsoft.Json.Linq;

namespace CodeGenerator
{
    public class CodeGenerator
    {
        public static void Process(string inputText, string inputPath, ICodeGenWriter writer)
        {
            var array = JArray.Parse(inputText);

            for (int i = 0; i < array.Count; i++)
            {
                var input = array[i] as JObject;
                if (input == null)
                    continue;

                var type = (string)input["Type"];

                ICodeGenModule module;
                switch (type)
                {
                    case "DbProc":
                        module = new DbProcModule();
                        break;
                    case "DbRowset":
                        module = new DbRowsetModule();
                        break;
                    case "DbSql":
                        module = new DbSqlModule();
                        break;
                    default:
                        module = null;
                        break;
                }

                if (module != null)
                {
                    try
                    {
                        module.Generate(input, inputPath, writer);
                    }
                    catch (Exception e)
                    {
                        JToken name;
                        input.TryGetValue("Name", out name);
                        Console.WriteLine("Exception name={0} type={1}", name, type);
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}
