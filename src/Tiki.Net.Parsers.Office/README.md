# Tiki.Net.Parsers.Office

Office document text and metadata extraction for [Tiki.Net](https://github.com/tomlm/Tiki.Net).

## Overview

Extracts text content and metadata from modern Office formats using the [Open XML SDK](https://github.com/dotnet/Open-XML-SDK), and OpenDocument formats via standard ZIP/XML parsing.

Supported formats:
- **DOCX** → `TikiDocument`
- **XLSX** → `TikiSpreadsheet`
- **PPTX** → `TikiPresentation`
- **ODT/ODS/ODP** → `TikiDocument`

## Installation

```bash
dotnet add package Tiki.Net.Parsers.Office
```

## Usage

```csharp
using Tiki;
using Tiki.Documents;

var tiki = new TikiEngine();

// Word document
var result = await tiki.ParseAsync("report.docx");
if (result is TikiDocument doc)
{
    Console.WriteLine($"Title: {doc.Title}");
    Console.WriteLine($"Company: {doc.Company}");
    Console.WriteLine($"Pages: {doc.PageCount}");
    Console.WriteLine($"Words: {doc.WordCount}");
}

// Spreadsheet
result = await tiki.ParseAsync("data.xlsx");
if (result is TikiSpreadsheet sheet)
{
    Console.WriteLine($"Content: {sheet.Content}"); // tab-separated cell values
}

// Presentation
result = await tiki.ParseAsync("deck.pptx");
if (result is TikiPresentation pres)
{
    Console.WriteLine($"Slides: {pres.SlideCount}");
    Console.WriteLine($"Content: {pres.Content}"); // text from all slides
}
```

## Links

- [Tiki.Net](https://github.com/tomlm/Tiki.Net) — main project
- [Open XML SDK](https://github.com/dotnet/Open-XML-SDK) — underlying OOXML library
