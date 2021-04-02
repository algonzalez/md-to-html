// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Yaml;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MD2Html
{
    static class MarkdigExtensionMethods
    {
        public static bool AnyCode(this MarkdownDocument mdDoc)
            => mdDoc.Descendants<Block>().Any(b
                => b is FencedCodeBlock
                    || (b is ParagraphBlock pblock && pblock.Inline.Descendants<CodeInline>().Any()));

        public static bool AnyMath(this MarkdownDocument mdDoc)
            => mdDoc.Descendants<Block>().Any(b
                => b is MathBlock
                    || (b is ParagraphBlock pblock && pblock.Inline.Descendants<MathInline>().Any()));

        public static bool AnyMermaidBlocks(this MarkdownDocument mdDoc)
            => mdDoc.Descendants<FencedCodeBlock>().Any(b
                => (b.Info ?? "").Trim().StartsWith("mermaid", StringComparison.OrdinalIgnoreCase));

        public static bool AnyTocBlocks(this MarkdownDocument mdDoc)
            => mdDoc.Descendants<ParagraphBlock>().Any(b => b.IsTocBlock());

        // TODO: ability to select range to include in TOC (ex. 2-4)
        public static string BuildHtmlToc(this MarkdownDocument mdDoc, string tocClass = "toc")
        {
            var sbToc = new System.Text.StringBuilder();
            var hBlocks = mdDoc.Descendants<HeadingBlock>();
            if (hBlocks.Any()) {
                sbToc.Append("    ");
                sbToc.AppendLine($"<div class=\"{tocClass}\">");
                int curLevel = 0;
                foreach (var block in hBlocks) {
                    if (block.Level > curLevel) {
                        sbToc.Append("    ");
                        sbToc.Append(new string(' ', curLevel * 2));
                        sbToc.AppendLine("<ul>");
                        curLevel = block.Level;
                    }
                    else if (curLevel > block.Level) {
                        curLevel = block.Level;
                        sbToc.Append("    ");
                        sbToc.Append(new string(' ', curLevel * 2));
                        sbToc.AppendLine("</ul>");
                    }
                    sbToc.Append("    ");
                    sbToc.Append(new string(' ', curLevel * 2));
                    sbToc.AppendLine($"<li><a href=\"#{block.GetAttributes().Id}\">{block?.Inline?.FirstChild}</a></li>");
                }
                while (curLevel > 0) {
                    curLevel--;
                    sbToc.Append("    ");
                    sbToc.Append(new string(' ', curLevel * 2));
                    sbToc.AppendLine("</ul>");
                }
                sbToc.Append("    ");
                sbToc.Append($"</div>");
            }
            return sbToc.ToString();
        }

        public static string GetFirstH1Text(this MarkdownDocument mdDoc)
        {
            var hBlock = mdDoc.Descendants<HeadingBlock>().FirstOrDefault(b => b.Level == 1);
            return hBlock?.Inline?.FirstChild?.ToString();
        }

        // NOTE: may consider using actual YAML parser to ensure proper string conversion
        //       this should be adequate for now, as this is just for the title
        public static string GetTitleFromYamlFrontMatter(this MarkdownDocument mdDoc)
        {
            var yamlBlock = mdDoc.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
            string title = (yamlBlock?.Lines.Lines
                    .Select(sl => sl.ToString())
                    .FirstOrDefault(s => s.TrimStart().StartsWith("title:", StringComparison.OrdinalIgnoreCase))
                        ?? "title:").Split(':')[1].Trim();

            if (title.Length == 0)
                return null;

            if (title.StartsWith('"') && title.EndsWith('"'))
                title = title[1..^1].Trim();
            else if (title.StartsWith("'") && title.EndsWith("'"))
                title = title[1..^1].Trim();

            return title.Length > 0 ? title : null;
        }

        public static bool IsTocBlock(this ParagraphBlock block)
        {
            Inline inline = block?.Inline?.FirstChild;
            if (inline == null) return false;

            // TODO: review Markdig code to see if there
            //       is a better way to get the text
            return (inline?.ToString() ?? "") == "["
                && (inline?.NextSibling?.ToString() ?? "")
                    .Equals("toc]", StringComparison.OrdinalIgnoreCase);
        }
    }
}
