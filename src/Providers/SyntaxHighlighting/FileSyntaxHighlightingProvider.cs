// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;

namespace MD2Html.Providers.SyntaxHighlighting
{
    class FileSyntaxHighlightingProvider : ISyntaxHighlightingProvider
    {
        readonly Lazy<string> _syntaxHighligher;

        public FileSyntaxHighlightingProvider(string fileName)
        {
            var fi = new FileInfo(fileName);
            if (!fi.Exists)
                throw new ArgumentException("Specified  Syntax Highligher file wasn't found");
            _syntaxHighligher = new Lazy<string>(() => File.ReadAllText(fi.FullName));
        }

        public string GetSyntaxHighLighter() => _syntaxHighligher.Value;
    }
}
