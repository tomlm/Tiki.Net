namespace Tiki.Documents;

/// <summary>
/// Abstract base class for office documents (word processing, spreadsheets, presentations).
/// Contains shared metadata properties common to all office formats.
/// </summary>
public abstract class TikiOfficeDocument : TikiFile
{
    /// <summary>
    /// The last person to modify the document.
    /// </summary>
    public string? LastAuthor { get; init; }

    /// <summary>
    /// The company associated with the document.
    /// </summary>
    public string? Company { get; init; }

    /// <summary>
    /// The manager associated with the document.
    /// </summary>
    public string? Manager { get; init; }

    /// <summary>
    /// The subject of the document.
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// The category of the document.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// The application that created the document.
    /// </summary>
    public string? ApplicationName { get; init; }

    /// <summary>
    /// The revision number of the document.
    /// </summary>
    public int? RevisionNumber { get; init; }
}
