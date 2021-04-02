// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace MD2Html.Providers.Style
{
    interface IStyleProvider { string GetStyle(); }

    class DefaultStyleProvider : IStyleProvider
    {
        readonly Lazy<string> _style;

        public DefaultStyleProvider()
        {
            _style = new Lazy<string>(() =>
                "    <style>\n"
                + new Gonzal.ManifestResourceManager().GetString("resources/default.css")
                + "    </style>");
        }

        public string GetStyle() => _style.Value;
    }
}
