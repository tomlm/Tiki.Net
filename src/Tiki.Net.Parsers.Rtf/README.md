# Tiki.Net.Parsers.Rtf

RTF text extraction for [Tiki.Net](https://github.com/tomlm/Tiki.Net).

## Overview

Extracts plain text content from Rich Text Format documents using [RtfPipe](https://github.com/erdomke/RtfPipe). Returns a `TikiDocument` result.

## Installation

```bash
dotnet add package Tiki.Net.Parsers.Rtf
```

## Usage

```csharp
using Tiki;
using Tiki.Documents;

var tiki = new TikiEngine();
var result = await tiki.ParseAsync("document.rtf");

if (result is TikiDocument doc)
{
    Console.WriteLine($"Content: {doc.Content}");
}
```

## Links

- [Tiki.Net](https://github.com/tomlm/Tiki.Net) — main project
- [RtfPipe](https://github.com/erdomke/RtfPipe) — underlying RTF library
