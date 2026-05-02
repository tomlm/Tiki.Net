using Tiki.Documents;
using Tiki.Mime;

namespace Tiki.Parser;

/// <summary>
/// Defines a parser that can extract content and metadata from a document stream.
/// </summary>
public interface IParser
{
    /// <summary>
    /// The set of media types this parser supports.
    /// </summary>
    IReadOnlySet<MediaType> SupportedTypes { get; }

    /// <summary>
    /// Parses the given stream and returns a strongly-typed result.
    /// </summary>
    /// <param name="stream">The document stream to parse.</param>
    /// <param name="context">Optional parse context for configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed parsed document result.</returns>
    Task<Documents.TikiFile> ParseAsync(
        Stream stream,
        ParseContext? context = null,
        CancellationToken cancellationToken = default);
}
