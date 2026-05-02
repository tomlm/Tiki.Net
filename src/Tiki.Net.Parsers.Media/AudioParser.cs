using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;

namespace Tiki.Parsers.Media;

/// <summary>
/// Parser for audio files (MP3, FLAC, WAV, OGG, M4A) using TagLibSharp.
/// </summary>
public sealed class AudioParser : AbstractParser
{
    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.AudioMpeg,
        MediaType.AudioFlac,
        MediaType.AudioWav,
        MediaType.AudioOgg,
        MediaType.AudioMp4
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = context?.FileName ?? "audio.mp3";
            using var file = TagLib.File.Create(new StreamFileAbstraction(fileName, stream));

            var tag = file.Tag;
            var properties = file.Properties;

            // Determine media type from codec
            var mediaType = DetectAudioType(file);

            return Task.FromResult<Documents.TikiFile>(new TikiMusic
            {
                Content = string.Empty,
                MediaType = mediaType,
                Title = NullIfEmpty(tag.Title),
                Authors = tag.Performers?.Length > 0 ? tag.Performers : null,
                Description = NullIfEmpty(tag.Comment),
                Keywords = tag.Genres?.Length > 0 ? tag.Genres : null,
                DateCreated = tag.Year > 0 ? new DateTime((int)tag.Year, 1, 1) : null,
                ContentLength = stream.CanSeek ? stream.Length : null,
                Artist = NullIfEmpty(tag.FirstPerformer),
                AlbumArtist = NullIfEmpty(tag.FirstAlbumArtist),
                Album = NullIfEmpty(tag.Album),
                Genre = tag.Genres?.Length > 0 ? tag.Genres : null,
                TrackNumber = tag.Track > 0 ? (int)tag.Track : null,
                DiscNumber = tag.Disc > 0 ? (int)tag.Disc : null,
                Year = tag.Year > 0 ? (int)tag.Year : null,
                Composer = NullIfEmpty(tag.FirstComposer),
                Duration = properties.Duration > TimeSpan.Zero ? properties.Duration : null,
                Bitrate = properties.AudioBitrate > 0 ? properties.AudioBitrate * 1000 : null,
                SampleRate = properties.AudioSampleRate > 0 ? properties.AudioSampleRate : null,
                Channels = properties.AudioChannels > 0 ? properties.AudioChannels : null,
                Codec = properties.Codecs?.FirstOrDefault()?.Description
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new ParseException($"Failed to parse audio file: {ex.Message}", ex);
        }
    }

    private static MediaType DetectAudioType(TagLib.File file)
    {
        var mimeType = file.MimeType;
        if (!string.IsNullOrEmpty(mimeType))
        {
            return mimeType switch
            {
                "taglib/mp3" or "audio/mpeg" => MediaType.AudioMpeg,
                "taglib/flac" or "audio/flac" => MediaType.AudioFlac,
                "taglib/wav" or "audio/wav" => MediaType.AudioWav,
                "taglib/ogg" or "audio/ogg" => MediaType.AudioOgg,
                "taglib/m4a" or "taglib/mp4" or "audio/mp4" => MediaType.AudioMp4,
                _ => MediaType.AudioMpeg
            };
        }
        return MediaType.AudioMpeg;
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
