// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// See LICENSE.txt file in the project root for full license information.

using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace MD2Html.Renderers
{
    class CustomParagraphRenderer : ParagraphRenderer
    {
        private readonly string _htmlForToc;
        public CustomParagraphRenderer(string htmlForToc)
            => _htmlForToc = htmlForToc;

        protected override void Write(HtmlRenderer renderer, ParagraphBlock block)
        {
            if (block.IsTocBlock())
                renderer.WriteLine(_htmlForToc);
            else
                base.Write(renderer, block);
        }
    }
}
