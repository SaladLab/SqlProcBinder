﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CodeWriter;
using CommandLine;
using Newtonsoft.Json;

namespace CodeGenerator
{
    public class Program
    {
        // "-s..\..\..\CodeGenerator.Tests\Sql\GenerateInt.sql;..\..\..\CodeGenerator.Tests\Sql\SumInt.sql" --target a.cs --nullable -n Sql
        // "-i..\..\..\CodeGenerator.Tests\Sql\@SqlProc.json" --target Output.cs -n Sql -p .*:sp\1
        private static int Main(string[] args)
        {
            // Parse command line options

            if (args.Length == 1 && args[0].StartsWith("@"))
            {
                var argFile = args[0].Substring(1);
                if (File.Exists(argFile))
                {
                    args = File.ReadAllLines(argFile);
                }
                else
                {
                    Console.WriteLine("File not found: " + argFile);
                    return 1;
                }
            }

            var parser = new Parser(config => config.HelpWriter = Console.Out);
            if (args.Length == 0)
            {
                parser.ParseArguments<Options>(new[] { "--help" });
                return 1;
            }

            Options options = null;

            try
            {
                var result = parser.ParseArguments<Options>(args)
                                   .WithParsed(r => { options = r; });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in parsing arguments: {0}", e);
                return 1;
            }

            if (options == null)
                return 1;

            // Run process !

            var procs = new List<DbProcDeclaration>();
            var rowsets = new List<DbRowsetDeclaration>();
            var tableTypes = new List<DbTableTypeDeclaration>();

            foreach (var sourceFile in options.Sources)
            {
                ReadSource(sourceFile, options, procs, rowsets, tableTypes);
            }

            foreach (var optionFile in options.OptionFiles)
            {
                try
                {
                    ReadOption(optionFile, options, procs, rowsets, tableTypes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in reading option {0}: {1}", optionFile, e);
                    return 1;
                }
            }

            MakeClassName(options, procs, rowsets, tableTypes);

            return Process(options, procs, rowsets, tableTypes);
        }

        private static void ReadSource(string sourceFile,
                                       Options options,
                                       List<DbProcDeclaration> procs,
                                       List<DbRowsetDeclaration> rowsets,
                                       List<DbTableTypeDeclaration> tableTypes)
        {
            var parser = new DbProcParser();
            var decls = parser.Parse(File.ReadAllText(sourceFile));

            decls.ForEach(d =>
            {
                d.Params.ForEach(p => p.Nullable = options.Nullable);
            });

            procs.AddRange(decls);
        }

        public class JsonOption
        {
            public JsonProc[] Procs;
            public JsonRowset[] Rowsets;
            public JsonTableType[] TableTypes;
        }

        public class JsonProc
        {
            public string ClassName;
            public string Path;
            public string Source;
            public bool? Nullable;
            public bool Return;
            public string Rowset;
            public bool RowsetFetch;
        }

        public class JsonRowset
        {
            public string Name;
            public string[] Fields;
        }

        public class JsonTableType
        {
            public string ClassName;
            public string Path;
            public string Source;
        }

        private static void ReadOption(string optionFile,
                                       Options options,
                                       List<DbProcDeclaration> procs,
                                       List<DbRowsetDeclaration> rowsets,
                                       List<DbTableTypeDeclaration> tableTypes)
        {
            var joption = JsonConvert.DeserializeObject<JsonOption>(File.ReadAllText(optionFile));
            var baseDir = Path.GetDirectoryName(optionFile);

            if (joption.Procs != null)
            {
                var parser = new DbProcParser();
                foreach (var jproc in joption.Procs)
                {
                    List<DbProcDeclaration> decls;

                    if (string.IsNullOrEmpty(jproc.Path) == false)
                        decls = parser.Parse(File.ReadAllText(Path.Combine(baseDir, jproc.Path)));
                    else if (string.IsNullOrEmpty(jproc.Source) == false)
                        decls = parser.Parse(jproc.Source);
                    else
                        throw new ArgumentException("Path or Source is required: " + jproc);

                    decls.ForEach(d =>
                    {
                        d.ClassName = jproc.ClassName;
                        d.Return = jproc.Return;
                        d.Rowset = jproc.Rowset;
                        d.RowsetFetch = jproc.RowsetFetch;
                        d.Params.ForEach(p => p.Nullable = jproc.Nullable ?? options.Nullable);
                    });
                    procs.AddRange(decls);
                }
            }

            if (joption.Rowsets != null)
            {
                foreach (var jrowset in joption.Rowsets)
                {
                    var rowset = new DbRowsetDeclaration();
                    rowset.ClassName = jrowset.Name;
                    rowset.Fields = jrowset.Fields.Select(s =>
                    {
                        var strs = s.Split();
                        if (strs[0].EndsWith("?"))
                        {
                            return new DbField
                            {
                                Type = strs[0].Substring(0, strs[0].Length - 1),
                                Name = strs[1],
                                Nullable = true
                            };
                        }
                        else
                        {
                            return new DbField
                            {
                                Type = strs[0],
                                Name = strs[1]
                            };
                        }
                    }).ToList();
                    rowsets.Add(rowset);
                }
            }

            if (joption.TableTypes != null)
            {
                var parser = new DbTableTypeParser();
                foreach (var jtype in joption.TableTypes)
                {
                    List<DbTableTypeDeclaration> decls;

                    if (string.IsNullOrEmpty(jtype.Path) == false)
                        decls = parser.Parse(File.ReadAllText(Path.Combine(baseDir, jtype.Path)));
                    else if (string.IsNullOrEmpty(jtype.Source) == false)
                        decls = parser.Parse(jtype.Source);
                    else
                        throw new ArgumentException("Path or Source is required: " + jtype);

                    decls.ForEach(d =>
                    {
                        d.ClassName = jtype.ClassName;
                    });
                    tableTypes.AddRange(decls);
                }
            }
        }

        private static void MakeClassName(Options options,
                                          IList<DbProcDeclaration> procs,
                                          IList<DbRowsetDeclaration> rowsets,
                                          IList<DbTableTypeDeclaration> tableTypes)
        {
            foreach (var proc in procs.Where(p => string.IsNullOrEmpty(p.ClassName)))
            {
                proc.ClassName = proc.ProcName;

                if (options.Patterns != null)
                    proc.ClassName = ReplaceName(options.Patterns, proc.ProcName);
            }

            foreach (var tableType in tableTypes.Where(p => string.IsNullOrEmpty(p.ClassName)))
            {
                tableType.ClassName = tableType.TypeName;

                if (options.Patterns != null)
                    tableType.ClassName = ReplaceName(options.Patterns, tableType.TypeName);
            }
        }

        private static string ReplaceName(IEnumerable<string> patterns, string name)
        {
            foreach (var pattern in patterns)
            {
                var ps = pattern.Split(':');
                if (ps.Length != 2)
                    throw new ArgumentException("Illegal pattern: " + pattern);

                var re = new Regex(ps[0]);
                name = re.Replace(name, ps[1], 1);
            }

            return name;
        }

        private static int Process(Options options,
                                   IList<DbProcDeclaration> procs,
                                   IList<DbRowsetDeclaration> rowsets,
                                   IList<DbTableTypeDeclaration> tableTypes)
        {
            // Generate code

            Console.WriteLine("- Generate code");

            var settings = new CodeWriterSettings(CodeWriterSettings.CSharpDefault);
            settings.TranslationMapping["`"] = "\"";

            var w = new CodeWriter.CodeWriter(settings);

            w.HeadLines = new List<string>
            {
                "// ------------------------------------------------------------------------------",
                "// <auto-generated>",
                "//     This code was generated by SqlProcBinder.CodeGenerator.",
                "//",
                "//     Changes to this file may cause incorrect behavior and will be lost if",
                "//     the code is regenerated.",
                "// </auto-generated>",
                "// ------------------------------------------------------------------------------",
                "",

                "using System;",
                "using System.Collections.Generic;",
                "using System.Data;",
                "using System.Data.Common;",
                "using System.Threading.Tasks;",
                "using SqlProcBinder;",
                ""
            };

            UsingHandle namespaceHandle = null;
            if (string.IsNullOrEmpty(options.Namespace) == false)
                namespaceHandle = w.OpenBlock(new[] { $"namespace {options.Namespace}" });

            var procCodeGen = new DbProcCodeGenerator();
            foreach (var call in procs)
            {
                procCodeGen.Generate(call, w);
            }

            var rowsetCodeGen = new DbRowsetCodeGenerator();
            foreach (var rowset in rowsets)
            {
                rowsetCodeGen.Generate(rowset, w);
            }

            var tableTypeCodeGen = new DbTableTypeCodeGenerator();
            foreach (var tableType in tableTypes)
            {
                tableTypeCodeGen.Generate(tableType, w);
            }

            if (namespaceHandle != null)
                namespaceHandle.Dispose();

            // save generated code

            Console.WriteLine("- Save code");

            if (w.WriteAllText(options.TargetFile, true) == false)
                Console.WriteLine("Nothing changed. Skip writing.");

            return 0;
        }

        private static bool SaveFileIfChanged(string path, string text)
        {
            if (File.Exists(path))
            {
                var existingText = File.ReadAllText(path);
                if (existingText == text)
                {
                    return false;
                }
            }
            File.WriteAllText(path, text);
            return true;
        }
    }
}
