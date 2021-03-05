// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// See LICENSE.txt file in the project root for full license information.

namespace MD2Html.Providers.SyntaxHighlighting
{
    class HighlightJsSyntaxHighlightingProvider : ISyntaxHighlightingProvider
    {
        public string GetSyntaxHighLighter() => @"    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/10.6.0/styles/default.min.css"">
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/10.6.0/highlight.min.js""></script>
    <script>hljs.highlightAll();</script>";
    }
}
