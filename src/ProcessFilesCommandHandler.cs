// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using MD2Html.Providers.Style;
using MD2Html.Providers.SyntaxHighlighting;

namespace MD2Html
{
    class ProcessFilesCommandHandler : ICommandHandler
    {
        public Task<int> InvokeAsync(InvocationContext ctx)
        {
            int result = ProcessFiles(
                ctx.ParseResult.ValueForArgument<List<string>>("files"),
                ctx.ParseResult.ValueForOption<bool>("--to-file"),
                ctx.ParseResult.ValueForOption<bool>("--overwrite"),
                ctx.ParseResult.ValueForOption<bool>("--content-only"),
                ctx.ParseResult.ValueForOption<bool>("--launch"),
                ctx.ParseResult.ValueForOption<bool>("--no-toc"),
                ctx.ParseResult.ValueForOption<string>("--out-dir"),
                ctx.ParseResult.ValueForOption<string>("--highlighter"),
                ctx.ParseResult.ValueForOption<string>("--toc-class"),
                ctx.ParseResult.ValueForOption<string[]>("--style")
            );

            return Task.FromResult(result);
        }

        private static int ProcessFiles(
            List<string> files
            , bool toFile
            , bool overwrite
            , bool contentOnly
            , bool launch
            , bool skipToc
            , string outDir
            , string highlighter
            , string tocClassName
            , string[] style)
        {
            MD2HtmlConverter converter;
            try {
                style ??= Array.Empty<string>();
                converter = new MD2HtmlConverter() {
                    OutputDirectory = outDir?.Trim(),
                    OutputToFile = toFile || outDir != null,
                    OverwriteIfExists = overwrite,
                    ContentOnly = contentOnly,
                    LaunchFile = launch,
                    SyntaxHighlightingProvider
                        = string.IsNullOrWhiteSpace(highlighter)
                            ? null
                            : new FileSyntaxHighlightingProvider(highlighter),
                    StyleProviders = new IStyleProvider[style.Length],
                    SkipToc = skipToc,
                    TocClassName = tocClassName
                };
                for (int i = 0; i < style.Length; i++) {
                    converter.StyleProviders[i]
                        = style[i].StartsWith("https://") || style[i].StartsWith("http://")
                            ? new UrlStyleProvider(style[i])
                            : new FileStyleProvider(style[i]);
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Oops: {ex.Message}");
                // Console.WriteLine(ex.StackTrace);
                return 1;
            }

            (int processedCount, int failedCount, string[] errorMessages)
                = converter.Convert(files.ToArray());

            int successCount = processedCount - failedCount;
            if (processedCount == 0)
                return 2;   // no file found
            if (successCount == 0)
                return 1;   // error
            if (errorMessages.Length > 0)
                return 3;   // some success

            return 0;
        }
    }
}
