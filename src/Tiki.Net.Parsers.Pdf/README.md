# Tiki.Net.Parsers.Pdf

PDF text and metadata extraction for [Tiki.Net](https://github.com/tomlm/Tiki.Net).

## Overview

Extracts text content and metadata (title, author, page count, creation date) from PDF documents using [PdfPig](https://github.com/UglyToad/PdfPig). Returns a `TikiDocument` result.

## Installation

```bash
dotnet add package Tiki.Net.Parsers.Pdf
```

## Usage

```csharp
using Tiki;
using Tiki.Documents;

var tiki = new TikiEngine();
var result = await tiki.ParseAsync("document.pdf");

if (result is TikiDocument doc)
{
    Console.WriteLine($"Title: {doc.Title}");
    Console.WriteLine($"Pages: {doc.PageCount}");
    Console.WriteLine($"Content: {doc.Content}");
}
```

## Extracted Properties

| Property | Type | Description |
|----------|------|-------------|
| Content | string | Full extracted text |
| Title | string? | Document title |
| Authors | string[]? | Document authors |
| PageCount | int? | Number of pages |
| DateCreated | DateTime? | Creation date |
| DateModified | DateTime? | Last modified date |
| ApplicationName | string? | PDF creator application |

## Links

- [Tiki.Net](https://github.com/tomlm/Tiki.Net) — main project
- [PdfPig](https://github.com/UglyToad/PdfPig) — underlying PDF library
