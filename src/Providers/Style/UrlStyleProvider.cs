// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// See LICENSE.txt file in the project root for full license information.

namespace MD2Html.Providers.Style
{
    class UrlStyleProvider : IStyleProvider
    {
        readonly System.Uri _styleUri;
        public UrlStyleProvider(string url)
        {
            _styleUri = new System.Uri(url);
        }

        public string GetStyle() => $"    <link rel=\"stylesheet\" href=\"{_styleUri.AbsoluteUri}\">";
    }
}
