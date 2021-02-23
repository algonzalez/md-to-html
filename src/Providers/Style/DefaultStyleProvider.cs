// Copyright 2021 Alberto Gonzalez. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt file in the project root for full license information.

namespace MD2Html.Providers.Style
{
    interface IStyleProvider { string GetStyle(); }

    class DefaultStyleProvider : IStyleProvider
    {
// TODO: add styling for blockquote
        public string GetStyle() =>
@"    <style>
      body {
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe WPC', 'Segoe UI', system-ui, 'Ubuntu', 'Droid Sans', sans-serif;
        font-size: 14px;
        line-height: 1.6;
      }
      table {
        border-collapse: collapse;
        padding: 0;
      }
      th, td {
        border: 1px solid black;
        padding: 1px 4px 1px 4px;
      }
    </style>";
    }
}
