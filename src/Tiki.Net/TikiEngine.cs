using Tiki.Detect;
using Tiki.Documents;
using Tiki.IO;
using Tiki.Mime;
using Tiki.Parser;

namespace Tiki;

/// <summary>
/// The primary facade for content detection and extraction.
/// Provides a simple API for parsing documents into strongly-typed results.
/// </summary>
public sealed class TikiEngine
{
    private readonly TikiConfig _config;

    /// <summary>
    /// Creates a new Tiki instance with the specified or default configuration.
    /// </summary>
    public TikiEngine(TikiConfig? config = null)
    {
        _config = config ?? TikiConfig.Default;
    }

    /// <summary>
    /// Maximum content length for text extraction. Default is unlimited.
    /// </summary>
    public int MaxStringLength { get; set; } = int.MaxValue;

    /// <summary>
    /// Parses a stream and returns a strongly-typed result.
    /// </summary>
    public Task<Documents.TikiFile> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return ParseAsync(stream, null, cancellationToken);
    }

    /// <summary>
    /// Parses a stream and returns a strongly-typed result, using the file name (if any) as a hint.
    /// </summary>
    /// <remarks>
    /// The content is examined first. The name decides the type only when the content cannot:
    /// text is read as CSV, JSON, XML and so on according to its extension, and a ZIP container
    /// as DOCX, XLSX and so on. Where the content has a signature of its own (PDF, JPEG, MP3, ...),
    /// or opens as an HTML document, that wins over an extension that disagrees, and text is not
    /// handed to a binary parser because of its name. A name with an unknown extension, or none,
    /// is treated as no name.
    /// </remarks>
    public async Task<Documents.TikiFile> ParseAsync(Stream stream, string? fileName, CancellationToken cancellationToken = default)
    {
        await using var tikaStream = await TikiStream.CreateAsync(stream, cancellationToken).ConfigureAwait(false);
        var context = new ParseContext
        {
            MaxContentLength = MaxStringLength,
            FileName = fileName
        };
        return await _config.Parser.ParseAsync(tikaStream, context, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Parses a file and returns a strongly-typed result.
    /// </summary>
    public async Task<Documents.TikiFile> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var tikaStream = TikiStream.Create(filePath);
        var context = new ParseContext
        {
            MaxContentLength = MaxStringLength,
            FileName = Path.GetFileName(filePath)
        };
        return await _config.Parser.ParseAsync(tikaStream, context, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Parses a stream and returns just the extracted text content.
    /// </summary>
    public async Task<string> ParseToStringAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var result = await ParseAsync(stream, cancellationToken).ConfigureAwait(false);
        return result.Content;
    }

    /// <summary>
    /// Parses a stream and returns just the extracted text content, using the file name (if any)
    /// as a hint. See <see cref="ParseAsync(Stream, string, CancellationToken)"/> for how the
    /// name and the content are weighed.
    /// </summary>
    public async Task<string> ParseToStringAsync(Stream stream, string? fileName, CancellationToken cancellationToken = default)
    {
        var result = await ParseAsync(stream, fileName, cancellationToken).ConfigureAwait(false);
        return result.Content;
    }

    /// <summary>
    /// Parses a file and returns just the extracted text content.
    /// </summary>
    public async Task<string> ParseToStringAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var result = await ParseAsync(filePath, cancellationToken).ConfigureAwait(false);
        return result.Content;
    }

    /// <summary>
    /// Detects the media type of a stream.
    /// </summary>
    public Task<MediaType> DetectAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return DetectAsync(stream, null, cancellationToken);
    }

    /// <summary>
    /// Detects the media type of a stream, using the file name (if any) as a hint. The content
    /// is examined first; the name decides only what the content cannot (which text format,
    /// which format inside a ZIP container), and never overrides a signature it disagrees with.
    /// See <see cref="ParseAsync(Stream, string, CancellationToken)"/>.
    /// </summary>
    public async Task<MediaType> DetectAsync(Stream stream, string? fileName, CancellationToken cancellationToken = default)
    {
        await using var tikaStream = await TikiStream.CreateAsync(stream, cancellationToken).ConfigureAwait(false);
        return await _config.Detector.DetectAsync(tikaStream, fileName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Detects the media type of a file.
    /// </summary>
    public async Task<MediaType> DetectAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var tikaStream = TikiStream.Create(filePath);
        var fileName = Path.GetFileName(filePath);
        return await _config.Detector.DetectAsync(tikaStream, fileName, cancellationToken).ConfigureAwait(false);
    }
}
