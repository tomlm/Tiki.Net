namespace Tiki.Documents;

/// <summary>
/// Represents parsed metadata for word processing documents
/// (PDF, DOCX, DOC, RTF).
/// </summary>
public class TikiDocument : TikiOfficeDocument
{
    /// <summary>
    /// The number of pages in the document.
    /// </summary>
    public int? PageCount { get; init; }

    /// <summary>
    /// The total word count.
    /// </summary>
    public int? WordCount { get; init; }

    /// <summary>
    /// The total character count.
    /// </summary>
    public int? CharacterCount { get; init; }
}
