using System;
using System.IO;
using System.Text;

namespace CodeGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                ShowUsage();
                return;
            }

            var source = args[0];
            var output = "";
            if (source.ToLower().EndsWith(".codegen.json"))
                output = source.Substring(0, source.Length - 13) + ".cs";
            else
                output = source.Substring(0, source.Length - Path.GetExtension(source).Length) + ".cs";

            Console.WriteLine("SOURCE: " + source);
            Console.WriteLine("OUTPUT: " + output);

            var input = File.ReadAllText(source);
            var writer = new TextCodeGenWriter();

            writer.AddUsing("System");
            writer.AddUsing("System.Collections.Generic");
            writer.AddUsing("System.Data");
            writer.AddUsing("System.Data.Common");
            writer.AddUsing("System.Data.SqlClient");
            writer.AddUsing("System.Threading.Tasks");

            writer.PushNamespace(Path.GetFileNameWithoutExtension(output));

            CodeGenerator.Process(input, source, writer);

            writer.PopNamespace();

            var code = writer.ToString();
            File.WriteAllText(output, code, Encoding.UTF8);

            Console.WriteLine("Done");
        }

        private static void ShowUsage()
        {
            Console.WriteLine("[Usage] CodeGen.exe SourceFile [OutputFile]");
        }
    }
}
