// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt file in the project root for full license information.

namespace MD2Html.Providers.SyntaxHighlighting
{
    interface ISyntaxHighlightingProvider { string GetSyntaxHighLighter(); }

    class DefaultSyntaxHighlightingProvider : ISyntaxHighlightingProvider
    {
        readonly ISyntaxHighlightingProvider _defaultHighlighter;

        public DefaultSyntaxHighlightingProvider()
            => _defaultHighlighter = new HighlightJsSyntaxHighlightingProvider();

        public string GetSyntaxHighLighter() => _defaultHighlighter.GetSyntaxHighLighter();
    }
}
