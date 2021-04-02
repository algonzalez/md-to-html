// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;

namespace MD2Html.Providers.Style
{
    class FileStyleProvider : IStyleProvider
    {
        readonly Lazy<string> _style;

        public FileStyleProvider(string fileName)
        {
            var fi = new FileInfo(fileName);
            if (!fi.Exists)
                throw new ArgumentException("Specified style file was not found");
            _style = new Lazy<string>(() =>
                "    <style>\n"
                + File.ReadAllText(fi.FullName)
                + "    </style>");
        }

        public string GetStyle() => _style.Value;
    }
}
