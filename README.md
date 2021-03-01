# md2html - Markdown to HTML

[![dot NET 5](https://img.shields.io/badge/.NET-5.0-orange.svg)](https://dotnet.microsoft.com/download/dotnet/5.0)
[![package license](https://img.shields.io/github/license/algonzalez/md-to-html.svg)](LICENSE.txt)
![supported platforms](https://img.shields.io/badge/platforms-windows-lightgray.svg)
![platforms pending testing](https://img.shields.io/badge/platforms%20pending%20testing-linux%20%7C%20macos-lightgray.svg)

md2html is a command-line interface (CLI) tool for converting markdown
formatted files into HTML. It wraps the [Markdig] markdown processing library
with a command-line driven UI.

I like to write my markdown documents in the [Visual Studio Code] editor with the 
help of the [Markdown All in One] and the [Markdown Preview Mermaid Support]
extensions. These extensions are great, but they are still missing a few 
useful additions to markdown. This is what [Markdig] adds to the mix. 
[Markdig] provides quite a few useful additions to markdown, 
like ^ for superscript, ~ for subscript, grid tables, 
definition lists, ordered lists with alpha characters 
or roman numerals and even emojis.

## Dependencies

- [Markdig] parses the markdown and renders the HTML.
- [System.CommandLine] parses command line arguments and options.

See the [LICENSE.txt](LICENSE.txt) file for copyright and license information for each dependency. 

## Usage

- `md2html --help` shows help with available options.
- `md2html HowTo.md` converts the file and writes the HTML to stdout.
- `md2html --to-file HowTo.md` results in a *HowTo.html* file in the same directory as the *HowTo<span>.</span>md* file. It will fail if the *HowTo.html* file already exists. Adding the `--overwrite` option will attempt to replace the file with the new HTML.
- `md2html --to-file HowTo.md CHANGELOG.md` will convert the two specified files. You can also use wildcards `md2html --to-file *.md` or mix and match `md2html --to-file HowTo.md docs\*.md`.
- `md2html --to-file --outdir htmlfiles HowTo.md` results in a *HowTo.html* file in the htmlfiles directory.
- `md2html --to-file HowTo.md --style custom.css` will merge the contents of the *custom.css* into the html file.
- `md2html --to-file HowTo.md --style https://cdnjs.cloudflare.com/ajax/libs/tufte-css/1.7.2/tufte.min.css` will include a stylesheet link tag pointing to the specified URL.

## Authors

**Alberto Gonzalez** (aka "Al")

  - Github: [http://github.com/algonzalez](http://github.com/algonzalez)
  - Twitter: [@AlGonzalez](http://twitter.com/algonzalez)

## Copyright & License

md2html is Copyright 2021 by Alberto Gonzalez, All Rights Reserved.

It is licensed under the Apache License, Version 2.0 (the "License"); you may not use this work except in compliance with the License. 

You may obtain a copy of the License in the [LICENSE.txt](LICENSE.txt) file, or at [https://opensource.org/licenses/Apache-2.0](https://opensource.org/licenses/Apache-2.0)


[Markdig]: https://github.com/xoofx/markdig
[Markdown All in One]: https://marketplace.visualstudio.com/items?itemName=yzhang.markdown-all-in-one
[Markdown Preview Mermaid Support]: https://marketplace.visualstudio.com/items?itemName=bierner.markdown-mermaid
[System.CommandLine]: https://github.com/dotnet/command-line-api
[Visual Studio Code]: https://code.visualstudio.com/
