// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using MD2Html.Providers.Style;
using MD2Html.Providers.SyntaxHighlighting;
using MD2Html.Renderers;

namespace MD2Html
{
    class MD2HtmlConverter
    {
        private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
                .UseEmojiAndSmiley()
                .UseYamlFrontMatter()       // used to extract the title
                .UseAdvancedExtensions()
                .Build();

        string _outputDirectory;
        string _tocClassName;
        IStyleProvider[] _styleProviders;
        ISyntaxHighlightingProvider _codeSyntaxHighlightingProvider;

        public bool ContentOnly { get; set; }

        public bool LaunchFile { get; set; }

        public string OutputDirectory
        {
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

        public IStyleProvider[] StyleProviders
        {
            get => _styleProviders.Length == 0
                    ? _styleProviders = new IStyleProvider[] { new DefaultStyleProvider() }
                    : _styleProviders;
            set => _styleProviders = value;
        }

        public ISyntaxHighlightingProvider SyntaxHighlightingProvider
        {
            get => _codeSyntaxHighlightingProvider ??= new DefaultSyntaxHighlightingProvider();
            set => _codeSyntaxHighlightingProvider = value;
        }

        public bool SkipMeta { get; set; }

        public bool SkipToc { get; set; }

        public string TocClassName
        {
            get => _tocClassName;
            set {
                value = (value ?? "").Trim();
                _tocClassName = value.Length > 0 ? value : "toc";
            }
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
            foreach (var filePath in filePaths) {
                var fi = new FileInfo(filePath);
                var files = Directory.GetFiles(fi.DirectoryName, fi.Name);
                foreach (var f in files)
                    filePathsToConvert.Add(f);
            }

            foreach (var filePath in filePathsToConvert) {
                var fi = new FileInfo(filePath);
                var outDir = OutputDirectory ?? fi.DirectoryName;
                var outputFilePath = Path.ChangeExtension(Path.Combine(outDir, fi.Name), ".html");

                try {
                    if (OutputToFile)
                        Console.WriteLine($">>> Converting: {filePath}");
                    ConvertFile(filePath, outputFilePath);
                    if (OutputToFile) {
                        Console.WriteLine($"    Success: {outputFilePath}");
                        if (LaunchFile) {
                            Process.Start(new ProcessStartInfo(outputFilePath) {
                                UseShellExecute = true
                            });
                        }
                    }
                }
                catch (Exception ex) {
                    failedCount++;
                    errorMessages.Add(ex.Message);
                    Console.Error.WriteLine($"    Error: {ex.Message}");
                }
                processedCount++;
            }

            if (OutputToFile && processedCount > 1) {
                Console.WriteLine($"{processedCount} processed; {processedCount - failedCount} succeeded; {failedCount} failed");
            }

            return (processedCount, failedCount, errorMessages.ToArray());
        }

        private void ConvertFile(string filePath, string outputFilePath)
        {
            var fi = new FileInfo(filePath);
            if (!fi.Exists)
                throw new ArgumentException("File was not found");

            if (OutputToFile && File.Exists(outputFilePath)) {
                if (OverwriteIfExists)
                    File.Delete(outputFilePath);
                else
                    throw new Exception("Output file already exists. Use the --overwrite option to replace it.");
            }

            string mdText = File.ReadAllText(fi.FullName);

            var mdDoc = Markdown.Parse(mdText, _pipeline);
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
                if (!SkipMeta) {
                    var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                    sw.WriteLine($"    <meta name=\"created-on\" content=\"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")}\">");
                    sw.WriteLine("    <meta name=\"created-with\" content=\"md2html\">");
                    sw.WriteLine("    <meta name=\"md2html:url\" content=\"https://github.com/algonzalez/md-to-html\">");
                    sw.WriteLine($"    <meta name=\"md2html:version\" content=\"{version}\">");
                }
                if (mdDoc.AnyCode()) {
                    sw.WriteLine(SyntaxHighlightingProvider.GetSyntaxHighLighter());
                }

                if (mdDoc.AnyMermaidBlocks()) {
                    sw.WriteLine("    <script src=\"https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js\"></script>");
                    sw.WriteLine("    <script>mermaid.initialize({startOnLoad:true});</script>");
                }

                if (mdDoc.AnyMath()) {
                    sw.WriteLine("    <script id=\"MathJax-script\" async src=\"https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js\"></script>");
                }

                foreach (var provider in StyleProviders) {
                    sw.WriteLine(provider.GetStyle());
                }
                sw.WriteLine("  </head>\n  <body>");
            }

            var renderer = new HtmlRenderer(sw);
            if (!SkipToc && mdDoc.AnyTocBlocks()) {
                var i = renderer.ObjectRenderers.FindIndex(r => r is ParagraphRenderer);
                if (i > -1)
                    renderer.ObjectRenderers.RemoveAt(i);
                renderer.ObjectRenderers.Add(new CustomParagraphRenderer(mdDoc.BuildHtmlToc(TocClassName)));
            }

            _pipeline.Setup(renderer);
            renderer.Render(mdDoc);

            if (!ContentOnly)
                sw.WriteLine("  </body>\n</html>");

            if (OutputToFile)
                File.WriteAllText(outputFilePath, sw.ToString());
            else
                Console.WriteLine(sw.ToString());
        }
    }
}
