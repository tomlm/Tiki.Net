namespace Tiki.Documents;

/// <summary>
/// Represents parsed metadata for spreadsheet documents (XLSX, XLS).
/// </summary>
public class TikiSpreadsheet : TikiOfficeDocument
{
    /// <summary>
    /// The number of sheets in the workbook.
    /// </summary>
    public int? SheetCount { get; init; }

    /// <summary>
    /// The names of the sheets in the workbook.
    /// </summary>
    public string[]? SheetNames { get; init; }
}
