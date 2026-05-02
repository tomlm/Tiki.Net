using Tiki.Documents;
using Tiki.Mime;

namespace Tiki.Parser;

/// <summary>
/// A parser that delegates to registered parsers based on media type.
/// </summary>
public sealed class CompositeParser : IParser
{
    private readonly Dictionary<MediaType, IParser> _parsers = new();

    public IReadOnlySet<MediaType> SupportedTypes => _parsers.Keys.ToHashSet();

    public CompositeParser(IEnumerable<IParser> parsers)
    {
        foreach (var parser in parsers)
        {
            foreach (var type in parser.SupportedTypes)
            {
                _parsers[type] = parser;
            }
        }
    }

    /// <summary>
    /// Gets the parser registered for the given media type.
    /// </summary>
    public IParser? GetParser(MediaType mediaType)
    {
        return _parsers.GetValueOrDefault(mediaType);
    }

    public Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        throw new UnsupportedFormatException("CompositeParser requires a media type to select a parser. Use AutoDetectParser instead.");
    }

    /// <summary>
    /// Parses the stream using the parser registered for the specified media type.
    /// </summary>
    public Task<Documents.TikiFile> ParseAsync(Stream stream, MediaType mediaType, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        var parser = _parsers.GetValueOrDefault(mediaType)
            ?? throw new UnsupportedFormatException($"No parser available for media type: {mediaType}");

        return parser.ParseAsync(stream, context, cancellationToken);
    }
}
