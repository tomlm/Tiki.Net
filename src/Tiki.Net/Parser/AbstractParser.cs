using Tiki.Documents;
using Tiki.Mime;

namespace Tiki.Parser;

/// <summary>
/// Base class for parser implementations providing common functionality.
/// </summary>
public abstract class AbstractParser : IParser
{
    public abstract IReadOnlySet<MediaType> SupportedTypes { get; }

    public abstract Task<Documents.TikiFile> ParseAsync(
        Stream stream,
        ParseContext? context = null,
        CancellationToken cancellationToken = default);
}
