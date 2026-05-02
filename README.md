[![Build and Test](https://github.com/tomlm/Tiki.Net/actions/workflows/BuildAndRunTests.yml/badge.svg)](https://github.com/tomlm/Tiki.Net/actions/workflows/BuildAndRunTests.yml)[![NuGet](https://img.shields.io/nuget/v/Tiki.Net.svg)](https://www.nuget.org/packages/Tiki.Net)

![Logo](https://raw.githubusercontent.com/tomlm/Tiki.Net/refs/heads/main/icon.png)

# Tiki.Net

A lightweight .NET library for content detection and text extraction, loosely inspired by [Apache Tika](https://tika.apache.org/). Tiki.Net is a native C# implementation — 
no IKVM, no Java, no bloat. It covers the most common file formats with strongly-typed results and a modern async API.

![Screenshot](https://raw.githubusercontent.com/tomlm/Tiki.Net/refs/heads/main/screenshot.gif)

## Why Tiki.Net?

- **Native .NET** — pure C#, no Java bridge, no 23 MB IKVM blob
- **Strongly-typed results** — get `TikiDocument`, `TikiPhoto`, `TikiMusic` objects with real properties, not string dictionaries
- **Lightweight** — core library is 57 KB; install only the parser packages you need
- **Modern** — async/await, CancellationToken, nullable reference types, net8.0+
- **Modular** — split NuGet packages keep your dependency footprint small

## Installation

```bash
# Core (includes text, XML, CSV, JSON parsing and MIME detection)
dotnet add package Tiki.Net

# Parser packages (install only what you need)
dotnet add package Tiki.Net.Parsers.Pdf       # PDF (via PdfPig)
dotnet add package Tiki.Net.Parsers.Office    # DOCX, XLSX, PPTX, ODT (via OpenXml SDK)
dotnet add package Tiki.Net.Parsers.Html      # HTML (via AngleSharp)
dotnet add package Tiki.Net.Parsers.Media     # MP3, FLAC, WAV, MP4, AVI, MKV, WMV (via TagLibSharp)
dotnet add package Tiki.Net.Parsers.Email     # EML (zero dependencies)
dotnet add package Tiki.Net.Parsers.Image     # JPEG, PNG, TIFF EXIF metadata (via MetadataExtractor)
dotnet add package Tiki.Net.Parsers.Rtf       # RTF (via RtfPipe)
```

## Usage

### Basic text extraction

```csharp
using Tiki;

var tiki = new Tiki.Tiki();

string text = await tiki.ParseToStringAsync("document.pdf");
```

### Strongly-typed parsing

```csharp
using Tiki;
using Tiki.Documents;

var tiki = new Tiki.Tiki();
var result = await tiki.ParseAsync("report.docx");

if (result is TikiDocument doc)
{
    Console.WriteLine($"Title: {doc.Title}");
    Console.WriteLine($"Author: {doc.Authors?.FirstOrDefault()}");
    Console.WriteLine($"Pages: {doc.PageCount}");
    Console.WriteLine($"Company: {doc.Company}");
    Console.WriteLine($"Content: {doc.Content[..200]}...");
}
```

### Image metadata

```csharp
var result = await tiki.ParseAsync("photo.jpg");

if (result is TikiPhoto photo)
{
    Console.WriteLine($"Camera: {photo.CameraManufacturer} {photo.CameraModel}");
    Console.WriteLine($"Date Taken: {photo.DateTaken}");
    Console.WriteLine($"ISO: {photo.IsoSpeed}, f/{photo.FNumber}");
    Console.WriteLine($"GPS: {photo.Latitude}, {photo.Longitude}");
}
```

### Music metadata

```csharp
var result = await tiki.ParseAsync("song.mp3");

if (result is TikiMusic music)
{
    Console.WriteLine($"Artist: {music.Artist}");
    Console.WriteLine($"Album: {music.Album}");
    Console.WriteLine($"Track: {music.TrackNumber}");
    Console.WriteLine($"Duration: {music.Duration}");
}
```

### Video metadata

```csharp
var result = await tiki.ParseAsync("movie.mp4");

if (result is TikiVideo video)
{
    Console.WriteLine($"Resolution: {video.Width}x{video.Height}");
    Console.WriteLine($"Duration: {video.Duration}");
    Console.WriteLine($"Codec: {video.VideoCodec}");
}
```

### Email parsing

```csharp
var result = await tiki.ParseAsync("message.eml");

if (result is TikiMessage msg)
{
    Console.WriteLine($"From: {msg.FromName} <{msg.FromAddress}>");
    Console.WriteLine($"To: {string.Join(", ", msg.ToAddresses ?? [])}");
    Console.WriteLine($"Subject: {msg.Subject}");
    Console.WriteLine($"Body: {msg.Content}");
}
```

### MIME type detection

```csharp
var mediaType = await tiki.DetectAsync("unknown-file.bin");
Console.WriteLine(mediaType); // e.g. "application/pdf"
```

### Custom configuration

```csharp
using Tiki;
using Tiki.Parsers.Pdf;
using Tiki.Parsers.Html;
using Tiki.Parsers.Media;

var config = TikiConfig.CreateBuilder()
    .AddPdfParser()
    .AddHtmlParser()
    .AddMediaParser()
    .Build();

var tiki = new Tiki.Tiki(config);
```

### Streaming and cancellation

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
using var stream = File.OpenRead("large-document.pdf");

var result = await tiki.ParseAsync(stream, cts.Token);
```

## Result Type Hierarchy

```
TikiFile                         Plain text, source code, markdown
├── TikiData                     JSON, XML, XAML, YAML, TOML, CSV, INI
├── TikiOfficeDocument (abstract)
│   ├── TikiDocument             PDF, DOCX, DOC, RTF, ODT
│   ├── TikiSpreadsheet          XLSX
│   └── TikiPresentation         PPTX
├── TikiMedia (abstract)
│   ├── TikiAudio
│   │   └── TikiMusic            MP3, FLAC, WAV, OGG
│   └── TikiVideo                MP4, AVI, MKV, MOV, WMV
├── TikiPhoto                    JPEG, PNG, TIFF, GIF, BMP, WebP
├── TikiMessage                  EML
└── TikiWebPage                  HTML
```

Common properties on `TikiFile`: `Content`, `MediaType`, `Title`, `Authors`, `DateCreated`, `DateModified`, `Description`, `Keywords`, `ContentLength`

## License

MIT
