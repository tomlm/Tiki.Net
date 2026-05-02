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
    public async Task<Documents.TikiFile> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        await using var tikaStream = await TikiStream.CreateAsync(stream, cancellationToken).ConfigureAwait(false);
        var context = new ParseContext { MaxContentLength = MaxStringLength };
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
    public async Task<MediaType> DetectAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        await using var tikaStream = await TikiStream.CreateAsync(stream, cancellationToken).ConfigureAwait(false);
        return await _config.Detector.DetectAsync(tikaStream, cancellationToken: cancellationToken).ConfigureAwait(false);
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
