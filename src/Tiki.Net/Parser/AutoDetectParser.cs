using Tiki.Detect;
using Tiki.Documents;
using Tiki.Mime;

namespace Tiki.Parser;

/// <summary>
/// A parser that automatically detects the document type and delegates to the appropriate parser.
/// </summary>
public sealed class AutoDetectParser : IParser
{
    private readonly IDetector _detector;
    private readonly CompositeParser _compositeParser;

    public AutoDetectParser(IDetector detector, CompositeParser compositeParser)
    {
        _detector = detector;
        _compositeParser = compositeParser;
    }

    public IReadOnlySet<MediaType> SupportedTypes => _compositeParser.SupportedTypes;

    public async Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        // Ensure stream is seekable for detection + parsing
        var startPosition = stream.CanSeek ? stream.Position : -1;

        // Detect media type
        var mediaType = await _detector.DetectAsync(stream, context?.FileName, cancellationToken).ConfigureAwait(false);

        // Reset stream position for parsing
        if (stream.CanSeek)
            stream.Position = startPosition;

        // If we detected ZIP, try to refine based on extension (could be OOXML)
        if (mediaType == MediaType.ApplicationZip && context?.FileName != null)
        {
            var ext = Path.GetExtension(context.FileName)?.ToLowerInvariant();
            mediaType = ext switch
            {
                ".docx" => MediaType.Docx,
                ".xlsx" => MediaType.Xlsx,
                ".pptx" => MediaType.Pptx,
                ".odt" => MediaType.Odt,
                ".ods" => MediaType.Ods,
                ".odp" => MediaType.Odp,
                _ => mediaType
            };
        }


        // Find and invoke the appropriate parser
        var parser = _compositeParser.GetParser(mediaType)
            ?? throw new UnsupportedFormatException($"No parser available for media type: {mediaType}");

        return await parser.ParseAsync(stream, context, cancellationToken).ConfigureAwait(false);
    }
}
