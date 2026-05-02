using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;

namespace Tiki.Parsers.Media;

/// <summary>
/// Parser for video files (MP4, AVI, MKV, MOV, WebM) using TagLibSharp.
/// </summary>
public sealed class VideoParser : AbstractParser
{
    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.VideoMp4,
        MediaType.VideoAvi,
        MediaType.VideoMkv,
        MediaType.VideoMov,
        MediaType.VideoWebm,
        MediaType.VideoWmv
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = context?.FileName ?? "video.mp4";
            using var file = TagLib.File.Create(new StreamFileAbstraction(fileName, stream));

            var tag = file.Tag;
            var properties = file.Properties;

            var mediaType = DetectVideoType(file, context?.FileName);

            return Task.FromResult<Documents.TikiFile>(new TikiVideo
            {
                Content = string.Empty,
                MediaType = mediaType,
                Title = NullIfEmpty(tag.Title),
                Authors = tag.Performers?.Length > 0 ? tag.Performers : null,
                Description = NullIfEmpty(tag.Comment),
                Keywords = tag.Genres?.Length > 0 ? tag.Genres : null,
                DateCreated = tag.Year > 0 ? new DateTime((int)tag.Year, 1, 1) : null,
                ContentLength = stream.CanSeek ? stream.Length : null,
                Duration = properties.Duration > TimeSpan.Zero ? properties.Duration : null,
                Bitrate = properties.AudioBitrate > 0 ? properties.AudioBitrate * 1000 : null,
                SampleRate = properties.AudioSampleRate > 0 ? properties.AudioSampleRate : null,
                Channels = properties.AudioChannels > 0 ? properties.AudioChannels : null,
                Codec = properties.Codecs?.FirstOrDefault()?.Description,
                Width = properties.VideoWidth > 0 ? properties.VideoWidth : null,
                Height = properties.VideoHeight > 0 ? properties.VideoHeight : null,
                FrameRate = null,
                VideoBitrate = null,
                VideoCodec = properties.Codecs?
                    .OfType<TagLib.IVideoCodec>()
                    .FirstOrDefault()?.Description
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new ParseException($"Failed to parse video file: {ex.Message}", ex);
        }
    }

    private static MediaType DetectVideoType(TagLib.File file, string? fileName)
    {
        var ext = fileName != null ? Path.GetExtension(fileName)?.ToLowerInvariant() : null;
        return ext switch
        {
            ".mp4" or ".m4v" => MediaType.VideoMp4,
            ".avi" => MediaType.VideoAvi,
            ".mkv" => MediaType.VideoMkv,
            ".mov" => MediaType.VideoMov,
            ".webm" => MediaType.VideoWebm,
            ".wmv" => MediaType.VideoWmv,
            _ => MediaType.VideoMp4
        };
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
