// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

namespace MD2Html
{
    static class MarkdigExtensionMethods
    {
        public static IEnumerable<Block> FindBlocksByType<TBlockType>(this MarkdownDocument mdDoc)
            where TBlockType : Block
        {
            return FindBlocksByType<TBlockType>((ContainerBlock)mdDoc);
        }

        private static IEnumerable<Block> FindBlocksByType<TBlockType>(Block block)
            where TBlockType : Block
        {
            if (block is ContainerBlock childBlocks) {
                foreach(var childBlock in childBlocks.SelectMany(FindBlocksByType<TBlockType>)) {
                    yield return childBlock;
                }
            }
            if (block is TBlockType)
                yield return block;
            else
                yield break;
        }

        public static string GetFirstH1Text(this MarkdownDocument mdDoc)
        {
            var hBlock = mdDoc.Descendants<HeadingBlock>().Where(b => b.Level == 1).FirstOrDefault();
            return hBlock?.Inline?.FirstChild?.ToString();
        }

        // NOTE: may consider using actual YAML parser to ensure proper string conversion
        //       this should be adequate for now, as this is just for the title
        public static string GetTitleFromYamlFrontMatter(this MarkdownDocument mdDoc)
        {
            var yamlBlock = mdDoc.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
            string title = (yamlBlock?.Lines.Lines
                    .Select(sl => sl.ToString())
                    .Where(s => s.TrimStart().StartsWith("title:", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault() ?? "title:").Split(':')[1].Trim();

            if (title.Length == 0)
                return null;

            if (title.StartsWith('"') && title.EndsWith('"'))
                title = title[1..^1].Trim();
            else if (title.StartsWith("'") && title.EndsWith("'"))
                title = title[1..^1].Trim();

            return title.Length > 0 ? title : null;
        }
    }
}
