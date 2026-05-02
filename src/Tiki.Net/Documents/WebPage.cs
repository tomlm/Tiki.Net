namespace Tiki.Documents;

/// <summary>
/// Represents parsed metadata for HTML web pages.
/// </summary>
public class TikiWebPage : TikiFile
{
    /// <summary>
    /// The document language (from html lang attribute or meta tag).
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// The generator meta tag value (e.g., "WordPress 6.0").
    /// </summary>
    public string? Generator { get; init; }

    /// <summary>
    /// The character encoding (e.g., "utf-8").
    /// </summary>
    public string? Charset { get; init; }

    /// <summary>
    /// Links found in the document.
    /// </summary>
    public string[]? Links { get; init; }
}
