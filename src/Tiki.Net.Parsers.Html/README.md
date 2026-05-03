# Tiki.Net.Parsers.Html

HTML text and metadata extraction for [Tiki.Net](https://github.com/tomlm/Tiki.Net).

## Overview

Parses HTML documents using [AngleSharp](https://github.com/AntisInc/AngleSharp), extracting visible text content, meta tags, and links. Returns a `TikiWebPage` result.

## Installation

```bash
dotnet add package Tiki.Net.Parsers.Html
```

## Usage

```csharp
using Tiki;
using Tiki.Documents;

var tiki = new TikiEngine();
var result = await tiki.ParseAsync("page.html");

if (result is TikiWebPage page)
{
    Console.WriteLine($"Title: {page.Title}");
    Console.WriteLine($"Language: {page.Language}");
    Console.WriteLine($"Generator: {page.Generator}");
    Console.WriteLine($"Links: {page.Links?.Length}");
    Console.WriteLine($"Content: {page.Content}");
}
```

## Extracted Properties

| Property | Type | Description |
|----------|------|-------------|
| Content | string | Visible body text (scripts/styles excluded) |
| Title | string? | From `<title>` tag |
| Authors | string[]? | From `<meta name="author">` |
| Description | string? | From `<meta name="description">` |
| Keywords | string[]? | From `<meta name="keywords">` |
| Language | string? | From `<html lang="">` or meta tag |
| Generator | string? | From `<meta name="generator">` |
| Charset | string? | Document character encoding |
| Links | string[]? | All `<a href="">` URLs found |

## Links

- [Tiki.Net](https://github.com/tomlm/Tiki.Net) — main project
- [AngleSharp](https://github.com/AngleSharp/AngleSharp) — underlying HTML parser
