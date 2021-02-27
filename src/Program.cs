// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MD2Html
{
    class Program
    {
        static int Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var rootCommand = ConfigureCommands(assembly);
            rootCommand.Handler = new ProcessFilesCommandHandler();
            var result = rootCommand.Parse(args);

            static bool IsHelpOption(string t) => t == "-h" || t == "--help" || t == "-?";
            if (result.Tokens.Select(t => t.Value).Any(IsHelpOption))
                WriteCopyrightHelpHeader(assembly);
            return rootCommand.Invoke(args);
        }

        private static RootCommand ConfigureCommands(Assembly assembly)
        {
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
            var rootCommand = new RootCommand(description);

            rootCommand.AddOption(new Option<bool>(new [] {"--to-file", "-f"},
                description: "Writes to files with markdown file name but with an .html extension"));

            rootCommand.AddOption(new Option<bool>(new [] {"--overwrite", "-o"},
                description: "Will overwrite the html file if it already exists"));

            rootCommand.AddOption(new Option<bool>(new [] {"--launch", "-l"},
                description: "Launch the html file in the associated application"));

            var outDirArgument =new Argument<string>("outputDir", "HEY,YOU!")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            outDirArgument.AddValidator(result =>
                !Directory.Exists(result.Tokens[0].Value)
                    ? $"No output directory matching '{result.Tokens[0].Value}'"
                    : null);
            var outDirOption = new Option<string>(new[] {"--out-dir", "-d"},
                description: "Directory to write the html files to; assumes --to-file flag")
            {
                Argument = outDirArgument
            };
            rootCommand.AddOption(outDirOption);

            var styleOption = new Option<string>(new [] {"--style", "-s"},
                description: "CSS file or URL to be added to the html output;\na file will be merged and a URL will be linked")
            {
                AllowMultipleArgumentsPerToken = false,
                Argument = new Argument<string>("styleFile")
                {
                    Arity = ArgumentArity.OneOrMore
                }
            };
            rootCommand.AddOption(styleOption);

            var highlighterFileArgument = new Argument<string>("highlighterFile")
            {
                Arity = ArgumentArity.ExactlyOne
            };
            highlighterFileArgument.AddValidator(result =>
                !File.Exists(result.Tokens[0].Value)
                    ? $"No file found matching '{result.Tokens[0].Value}'"
                    : null);
            var syntaxHightlighterOption = new Option<string>("--highlighter",
                description: "File containing tags to include for syntax highlighting support")
            {
                AllowMultipleArgumentsPerToken = false,
                Argument = highlighterFileArgument
            };
            rootCommand.AddOption(syntaxHightlighterOption);

            rootCommand.AddOption(new Option<bool>("--content-only",
                description: "Output content with no html, head, or body tags"));

            var filesArgument = new Argument<List<string>>("files",
                "Markdown files to convert to HTML - supports wildcards") {
                    Arity = new ArgumentArity(1, 16),
                };
            filesArgument.AddValidator(result => {
                var fi = new FileInfo(result.Tokens[0].Value);
                return (!fi.Exists && Directory.GetFiles(fi.DirectoryName).Length == 0)
                    ? $"No files found matching '{result.Tokens[0].Value}'"
                    : null;
                });

            rootCommand.AddArgument(filesArgument);

            return rootCommand;
        }

        private static void WriteCopyrightHelpHeader(Assembly assembly)
        {
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>().Configuration;
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            Console.Write($"{title} v{version}");
            Console.WriteLine(configuration == "Debug" ? " [Debug]" : "");
            Console.WriteLine(copyright);
            Console.WriteLine();
        }
    }
}
