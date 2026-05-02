namespace Tiki.Parser;

/// <summary>
/// Provides context and configuration for a parsing operation.
/// </summary>
public sealed class ParseContext
{
    private readonly Dictionary<Type, object> _context = new();

    /// <summary>
    /// Maximum length of extracted content. Default is unlimited.
    /// </summary>
    public int MaxContentLength { get; init; } = int.MaxValue;

    /// <summary>
    /// Maximum depth for recursively parsing embedded documents.
    /// </summary>
    public int MaxDepth { get; init; } = 10;

    /// <summary>
    /// The file name hint (used for extension-based detection).
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Gets a context object by type.
    /// </summary>
    public T? Get<T>() where T : class
    {
        return _context.TryGetValue(typeof(T), out var value) ? (T)value : null;
    }

    /// <summary>
    /// Sets a context object by type.
    /// </summary>
    public void Set<T>(T value) where T : class
    {
        _context[typeof(T)] = value;
    }
}
