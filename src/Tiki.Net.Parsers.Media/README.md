# Tiki.Net.Parsers.Media

Audio and video metadata extraction for [Tiki.Net](https://github.com/tomlm/Tiki.Net).

## Overview

Extracts metadata from audio and video files using [TagLibSharp](https://github.com/mono/taglib-sharp). Returns `TikiMusic` for audio with ID3/Vorbis tags, or `TikiVideo` for video containers.

Supported formats:
- **Audio**: MP3, FLAC, WAV, OGG, AAC, M4A
- **Video**: MP4, AVI, MKV, MOV, WMV, WebM

## Installation

```bash
dotnet add package Tiki.Net.Parsers.Media
```

## Usage

```csharp
using Tiki;
using Tiki.Documents;

var tiki = new TikiEngine();

// Music
var result = await tiki.ParseAsync("song.mp3");
if (result is TikiMusic music)
{
    Console.WriteLine($"Artist: {music.Artist}");
    Console.WriteLine($"Album: {music.Album}");
    Console.WriteLine($"Track: {music.TrackNumber}");
    Console.WriteLine($"Year: {music.Year}");
    Console.WriteLine($"Genre: {string.Join(", ", music.Genre ?? [])}");
    Console.WriteLine($"Duration: {music.Duration}");
    Console.WriteLine($"Bitrate: {music.Bitrate / 1000} kbps");
}

// Video
result = await tiki.ParseAsync("movie.mp4");
if (result is TikiVideo video)
{
    Console.WriteLine($"Resolution: {video.Width}x{video.Height}");
    Console.WriteLine($"Duration: {video.Duration}");
    Console.WriteLine($"Codec: {video.VideoCodec}");
}
```

## Links

- [Tiki.Net](https://github.com/tomlm/Tiki.Net) — main project
- [TagLibSharp](https://github.com/mono/taglib-sharp) — underlying media tag library
