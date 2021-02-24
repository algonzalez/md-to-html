// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using MD2Html.Providers.SyntaxHighlighting;
using MD2Html.Providers.Style;

namespace MD2Html
{
    class MD2HtmlConverter
    {
        public static readonly int DefaultMaxFileSizeSupported = 64*1024; // 64K
        public static readonly int MinFileSizeSupported = 4*1024;         //  4K

        int _maxFileSizeSupported = DefaultMaxFileSizeSupported;
        string _outputDirectory = null;
        IStyleProvider[] _styleProviders;
        ISyntaxHighlightingProvider _codeSyntaxHighlightingProvider;

        public bool ContentOnly { get; set; }

        public int MaxFileSizeSupported {
            get => _maxFileSizeSupported;
            set => _maxFileSizeSupported = value < MinFileSizeSupported
                ? MinFileSizeSupported
                : value;
        }

        public string OutputDirectory {
            get => _outputDirectory;
            set {
                value = value?.Trim();
                if (value != null && !Directory.Exists(value))
                    throw new ArgumentException("Output directory was not found");
                _outputDirectory = value;
            }
        }

        public bool OutputToFile { get; set; }

        public bool OverwriteIfExists { get; set; }

        public IStyleProvider[] StyleProviders {
            get => _styleProviders.Length == 0
                    ? _styleProviders = new [] { new DefaultStyleProvider() }
                    : _styleProviders;
            set => _styleProviders = value;
        }

        public ISyntaxHighlightingProvider SyntaxHighlightingProvider {
            get => _codeSyntaxHighlightingProvider ??= new DefaultSyntaxHighlightingProvider();
            set => _codeSyntaxHighlightingProvider = value;
        }

        public (int processedCount, int failedCount, string[] errorMessages)
            Convert(params string[] filePaths)
        {
            int processedCount = 0;
            int failedCount = 0;
            var errorMessages = new List<string>();

            if (filePaths.Length == 0)
                return (processedCount, failedCount, Array.Empty<string>());

            var filePathsToConvert = new HashSet<string>();
            foreach(var filePath in filePaths) {
                var fi = new FileInfo(filePath);
                var files = Directory.GetFiles(fi.DirectoryName, fi.Name);
                foreach (var f in files)
                    filePathsToConvert.Add(f);
            }

            foreach (var filePath in filePathsToConvert) {
                var fi = new FileInfo(filePath);
                var outDir = OutputDirectory ?? fi.DirectoryName;
                var outputFilePath = Path.ChangeExtension(Path.Combine(outDir, fi.Name), ".html");

                try
                {
                    if (OutputToFile)
                        Console.WriteLine($">>> Converting: {filePath}");
                    ConvertFile(filePath, outputFilePath);
                    if (OutputToFile)
                        Console.WriteLine($"    Success: {outputFilePath}");
                }
                catch (Exception ex)
                {
                    failedCount++;
                    errorMessages.Add(ex.Message);
                    Console.Error.WriteLine($"    Error: {ex.Message}");
                }
                processedCount++;
            }

            if (OutputToFile && processedCount > 1) {
                Console.WriteLine($"{processedCount} processed; {processedCount-failedCount} succeeded; {failedCount} failed");
            }

            return (processedCount, failedCount, errorMessages.ToArray());
        }

        private void ConvertFile(string filePath, string outputFilePath)
        {
            var fi = new FileInfo(filePath);
            if (!fi.Exists)
                throw new ArgumentException("File was not found");

            if (fi.Length > DefaultMaxFileSizeSupported)
                throw new Exception($"Only supports converting files upto {MaxFileSizeSupported/1024}K in size");

            string mdText = File.ReadAllText(fi.FullName);

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .Build();

            var mdDoc = Markdown.Parse(mdText, pipeline);
            string title = mdDoc.GetTitleFromYamlFrontMatter()
                ?? mdDoc.GetFirstH1Text()
                ?? Path.GetFileNameWithoutExtension(fi.FullName);

            var sw = new StringWriter();

            if (!ContentOnly) {
                sw.WriteLine($@"<!DOCTYPE html>
<html>
  <head>
    <title>
      {title}
    </title>");
                if (mdDoc.FindBlocksByType<FencedCodeBlock>().Any())
                    sw.WriteLine(SyntaxHighlightingProvider.GetSyntaxHighLighter());

                foreach (var provider in StyleProviders) {
                    sw.WriteLine(provider.GetStyle());
                }
                sw.WriteLine("  </head>\n  <body>");
            }
            var renderer = new HtmlRenderer(sw);
            pipeline.Setup(renderer);
            renderer.Render(mdDoc);

            if (!ContentOnly)
                sw.WriteLine("  </body>\n</html>");

            if (!OutputToFile) {
                Console.WriteLine(sw.ToString());
                return;
            }

            if (File.Exists(outputFilePath)){
                if (OverwriteIfExists)
                    File.Delete(outputFilePath);
                else
                    throw new Exception("Output file already exists");
            }

            File.WriteAllText(outputFilePath, sw.ToString());
        }
    }
}
