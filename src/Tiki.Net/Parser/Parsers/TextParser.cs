using System.Text;
using Tiki.Documents;
using Tiki.Mime;
using UtfUnknown;

namespace Tiki.Parser.Parsers;

/// <summary>
/// Parser for plain text files with automatic encoding detection.
/// </summary>
public sealed class TextParser : AbstractParser
{
    private static readonly MediaType s_csv = MediaType.Parse("text/csv");
    private static readonly MediaType s_tsv = MediaType.Parse("text/tab-separated-values");

    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.TextPlain,
        s_csv,
        s_tsv,
        MediaType.ApplicationJson
    };

    private static readonly HashSet<string> s_dataExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".json", ".csv", ".tsv", ".yaml", ".yml", ".toml", ".xml", ".xaml", ".ini", ".cfg", ".conf"
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override async Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        var maxLength = context?.MaxContentLength ?? int.MaxValue;

        // Detect encoding
        var encoding = await DetectEncodingAsync(stream, cancellationToken).ConfigureAwait(false);
        stream.Position = 0;

        // Read content
        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var content = maxLength == int.MaxValue
            ? await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false)
            : await ReadMaxLengthAsync(reader, maxLength, cancellationToken).ConfigureAwait(false);

        var mediaType = MediaType.TextPlain;
        var isData = false;

        if (context?.FileName != null)
        {
            var ext = Path.GetExtension(context.FileName);
            if (!string.IsNullOrEmpty(ext) && s_dataExtensions.Contains(ext))
                isData = true;

            // Determine specific media type from extension
            mediaType = ext?.ToLowerInvariant() switch
            {
                ".json" => MediaType.ApplicationJson,
                ".csv" => s_csv,
                ".tsv" => s_tsv,
                _ => MediaType.TextPlain
            };
        }

        if (isData)
        {
            return new Documents.TikiData
            {
                Content = content,
                MediaType = mediaType,
                ContentLength = stream.CanSeek ? stream.Length : null
            };
        }

        return new Documents.TikiFile
        {
            Content = content,
            MediaType = mediaType,
            ContentLength = stream.CanSeek ? stream.Length : null
        };
    }

    private static async Task<Encoding> DetectEncodingAsync(Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            var result = await Task.Run(() => CharsetDetector.DetectFromStream(stream), cancellationToken).ConfigureAwait(false);
            if (result.Detected?.Encoding != null)
                return result.Detected.Encoding;
        }
        catch
        {
            // Fall through to default
        }

        return Encoding.UTF8;
    }

    private static async Task<string> ReadMaxLengthAsync(StreamReader reader, int maxLength, CancellationToken cancellationToken)
    {
        var buffer = new char[Math.Min(maxLength, 8192)];
        var sb = new StringBuilder();
        int read;

        while (sb.Length < maxLength &&
               (read = await reader.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, maxLength - sb.Length)), cancellationToken).ConfigureAwait(false)) > 0)
        {
            sb.Append(buffer, 0, read);
        }

        return sb.ToString();
    }
}
