# md2html - Markdown to HTML

[![dot NET 5](https://img.shields.io/badge/.NET-5.0-orange.svg)](https://dotnet.microsoft.com/download/dotnet/5.0)
[![package license](https://img.shields.io/github/license/algonzalez/md-to-html.svg)](LICENSE.txt)
![supported platforms](https://img.shields.io/badge/platforms-windows-lightgray.svg)
![platforms pending testing](https://img.shields.io/badge/platforms%20pending%20testing-linux%20%7C%20macos-lightgray.svg)

A command line tool to convert markdown formatted files to HTML.

**Idea:** Combine with [wkhtmltopdf](https://wkhtmltopdf.org/) to go from Markdown to PDF.

## Dependencies

- [Markdig](https://github.com/xoofx/markdig) parses the markdown and renders the HTML.
- [System.CommandLine](https://github.com/dotnet/command-line-api) parses command line arguments and options.

See the [LICENSE.txt](LICENSE.txt) file for copyright and license information for each dependency. 

## Usage

Convert a file and write the html to stdout, then redirect the stdout to a file.
```
md2html README.md > README.html
```

Convert all the files with an '.md' extension. Files will be written to the same directory but with an '.html' extenstion.
```
md2html --to-file --overwrite *.md
```

The previous options can be shortened and combined.
```
md2html -fo *.md
```

Convert all the files witn an '.md' extension, but write to the 'htmlfiles' directory (directory must already exist).
```
md2html -fo *.md --outdir htmlfiles
```

Convert a file and specify a style sheet that will be written in to the <head> area of the html file.
```
md2html -fo README.md --style custom.css
```

## Authors

**Alberto Gonzalez** (aka "Al")

  - Github: [http://github.com/algonzalez](http://github.com/algonzalez)
  - Twitter: [@AlGonzalez](http://twitter.com/algonzalez)

## Copyright & License

md2html is Copyright 2021 by Alberto Gonzalez, All Rights Reserved.

It is licensed under the Apache License, Version 2.0 (the "License"); you may not use this work except in compliance with the License. 

You may obtain a copy of the License in the [LICENSE.txt](LICENSE.txt) file, or at [https://opensource.org/licenses/Apache-2.0](https://opensource.org/licenses/Apache-2.0)
