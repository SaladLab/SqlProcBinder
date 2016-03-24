using System.Collections.Generic;
using CommandLine;

namespace CodeGenerator
{
    internal class Options
    {
        [Option('i', "input", Separator = ';',
                HelpText = "Input option files.")]
        public IEnumerable<string> OptionFiles { get; set; }

        [Option('s', "source", Separator = ';',
                HelpText = "Input stored procedure files.")]
        public IEnumerable<string> Sources { get; set; }

        [Option('p', "pattern", Separator = ';',
                HelpText = "Regular expression  pattern to make class name from procedure name.")]
        public IEnumerable<string> Patterns { get; set; }

        [Option('t', "target", Required = true,
                HelpText = "Filename of a generated code.")]
        public string TargetFile { get; set; }

        [Option('n', "namespace",
                HelpText = "Namespace that generated source has.")]
        public string Namespace { get; set; }
    }
}
