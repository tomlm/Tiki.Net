# Tiki.Net.Parsers.Image

Image EXIF/IPTC metadata extraction for [Tiki.Net](https://github.com/tomlm/Tiki.Net).

## Overview

Extracts EXIF, IPTC, and XMP metadata from image files using [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet). Returns a `TikiPhoto` result with camera info, GPS coordinates, and exposure data.

Supported formats: JPEG, PNG, TIFF, GIF, BMP, WebP

## Installation

```bash
dotnet add package Tiki.Net.Parsers.Image
```

## Usage

```csharp
using Tiki;
using Tiki.Documents;

var tiki = new TikiEngine();
var result = await tiki.ParseAsync("photo.jpg");

if (result is TikiPhoto photo)
{
    Console.WriteLine($"Camera: {photo.CameraManufacturer} {photo.CameraModel}");
    Console.WriteLine($"Lens: {photo.LensModel}");
    Console.WriteLine($"Date: {photo.DateTaken}");
    Console.WriteLine($"ISO: {photo.IsoSpeed}");
    Console.WriteLine($"Aperture: f/{photo.FNumber}");
    Console.WriteLine($"Shutter: {photo.ExposureTime}s");
    Console.WriteLine($"Focal Length: {photo.FocalLength}mm");
    Console.WriteLine($"GPS: {photo.Latitude}, {photo.Longitude}");
    Console.WriteLine($"Size: {photo.Width}x{photo.Height}");
}
```

## Extracted Properties

| Property | Type | Description |
|----------|------|-------------|
| CameraManufacturer | string? | Camera make (e.g., "Canon") |
| CameraModel | string? | Camera model (e.g., "EOS R5") |
| LensModel | string? | Lens model |
| DateTaken | DateTime? | When the photo was taken |
| ExposureTime | double? | Shutter speed in seconds |
| FNumber | double? | Aperture f-number |
| IsoSpeed | int? | ISO sensitivity |
| FocalLength | double? | Focal length in mm |
| Orientation | int? | EXIF orientation (1-8) |
| Width | int? | Image width in pixels |
| Height | int? | Image height in pixels |
| Latitude | double? | GPS latitude (decimal degrees) |
| Longitude | double? | GPS longitude (decimal degrees) |
| Flash | string? | Flash mode/status |
| WhiteBalance | string? | White balance setting |
| MeteringMode | string? | Metering mode |
| ExposureBias | double? | Exposure compensation in EV |

## Links

- [Tiki.Net](https://github.com/tomlm/Tiki.Net) — main project
- [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) — underlying EXIF library
