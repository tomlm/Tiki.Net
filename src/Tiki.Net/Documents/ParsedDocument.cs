using Tiki.Mime;

namespace Tiki.Documents;

/// <summary>
/// Base class for all parsed content results. Used directly for plain text,
/// source code, markdown, JSON, CSV, XML, and other simple text formats.
/// </summary>
public class TikiFile
{
    /// <summary>
    /// Path to the folder containing the file, if applicable. 
    /// </summary>
    public string? FolderPath { get; set; }

    /// <summary>
    /// Full path to the file, URL, or other identifier for the source of the content.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// The name of the file (extracted from the path or URL if not explicitly provided).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The extracted text content from the file.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// The detected media type of the file.
    /// </summary>
    public required MediaType MediaType { get; init; }

    /// <summary>
    /// The title of the file.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// The authors of the file.
    /// </summary>
    public string[]? Authors { get; init; }

    /// <summary>
    /// The date the file was created.
    /// </summary>
    public DateTime? DateCreated { get; init; }

    /// <summary>
    /// The date the file was last modified.
    /// </summary>
    public DateTime? DateModified { get; init; }

    /// <summary>
    /// A description or summary of the file.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Keywords or tags associated with the file.
    /// </summary>
    public string[]? Keywords { get; init; }

    /// <summary>
    /// The size of the file in bytes.
    /// </summary>
    public long? ContentLength { get; init; }
}
