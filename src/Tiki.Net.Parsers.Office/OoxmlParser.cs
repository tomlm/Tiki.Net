using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;
using Tiki.Documents;

namespace Tiki.Parsers.Office;

/// <summary>
/// Parser for Office Open XML documents (DOCX, XLSX, PPTX) using the Open XML SDK.
/// </summary>
public sealed class OoxmlParser : AbstractParser
{
    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.Docx,
        MediaType.Xlsx,
        MediaType.Pptx
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        var maxLength = context?.MaxContentLength ?? int.MaxValue;
        var fileName = context?.FileName;
        var extension = fileName != null ? Path.GetExtension(fileName)?.ToLowerInvariant() : null;

        try
        {
            // Determine type from extension first
            if (extension == ".xlsx")
                return Task.FromResult(ParseSpreadsheet(stream, maxLength, cancellationToken));

            if (extension == ".pptx")
                return Task.FromResult(ParsePresentation(stream, maxLength, cancellationToken));

            if (extension == ".docx")
                return Task.FromResult(ParseWordDocument(stream, maxLength, cancellationToken));

            // No extension hint - try each format
            if (TryOpenSpreadsheet(stream))
            {
                if (stream.CanSeek) stream.Position = 0;
                return Task.FromResult(ParseSpreadsheet(stream, maxLength, cancellationToken));
            }
            if (stream.CanSeek) stream.Position = 0;

            if (TryOpenPresentation(stream))
            {
                if (stream.CanSeek) stream.Position = 0;
                return Task.FromResult(ParsePresentation(stream, maxLength, cancellationToken));
            }
            if (stream.CanSeek) stream.Position = 0;

            // Default to word processing
            return Task.FromResult(ParseWordDocument(stream, maxLength, cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not TikiException)
        {
            if (ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("encrypt", StringComparison.OrdinalIgnoreCase))
                throw new EncryptedDocumentException("The document is encrypted or password-protected.", ex);

            throw new ParseException($"Failed to parse OOXML document: {ex.Message}", ex);
        }
    }

    private Documents.TikiFile ParseWordDocument(Stream stream, int maxLength, CancellationToken cancellationToken)
    {
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document?.Body;

        var sb = new StringBuilder();
        if (body != null)
        {
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (sb.Length >= maxLength) break;

                var text = paragraph.InnerText;
                if (!string.IsNullOrEmpty(text))
                {
                    if (sb.Length > 0) sb.Append('\n');
                    sb.Append(text);
                }
            }
        }

        // Extract core properties
        var props = doc.PackageProperties;
        var extProps = doc.ExtendedFilePropertiesPart?.Properties;

        int? pageCount = null;
        int? wordCount = null;
        int? charCount = null;
        string? company = null;
        string? appName = null;

        if (extProps != null)
        {
            if (int.TryParse(extProps.Pages?.Text, out var pages)) pageCount = pages;
            if (int.TryParse(extProps.Words?.Text, out var words)) wordCount = words;
            if (int.TryParse(extProps.Characters?.Text, out var chars)) charCount = chars;
            company = NullIfEmpty(extProps.Company?.Text);
            appName = NullIfEmpty(extProps.Application?.Text);
        }

        return new TikiDocument
        {
            Content = sb.Length > maxLength ? sb.ToString(0, maxLength) : sb.ToString(),
            MediaType = MediaType.Docx,
            Title = NullIfEmpty(props.Title),
            Authors = props.Creator != null ? new[] { props.Creator } : null,
            Description = NullIfEmpty(props.Description),
            Subject = NullIfEmpty(props.Subject),
            Keywords = ParseKeywords(props.Keywords),
            DateCreated = props.Created,
            DateModified = props.Modified,
            LastAuthor = NullIfEmpty(props.LastModifiedBy),
            Category = NullIfEmpty(props.Category),
            PageCount = pageCount,
            WordCount = wordCount,
            CharacterCount = charCount,
            Company = company,
            ApplicationName = appName,
            ContentLength = stream.CanSeek ? stream.Length : null
        };
    }

    private Documents.TikiFile ParseSpreadsheet(Stream stream, int maxLength, CancellationToken cancellationToken)
    {
        using var doc = SpreadsheetDocument.Open(stream, false);
        var workbookPart = doc.WorkbookPart;
        var sb = new StringBuilder();

        if (workbookPart != null)
        {
            var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

            foreach (var worksheetPart in workbookPart.WorksheetParts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (sb.Length >= maxLength) break;

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                if (sheetData == null) continue;

                foreach (var row in sheetData.Elements<Row>())
                {
                    if (sb.Length >= maxLength) break;

                    var cells = new List<string>();
                    foreach (var cell in row.Elements<Cell>())
                    {
                        var value = GetCellValue(cell, sharedStrings);
                        if (!string.IsNullOrEmpty(value))
                            cells.Add(value);
                    }

                    if (cells.Count > 0)
                    {
                        if (sb.Length > 0) sb.Append('\n');
                        sb.Append(string.Join('\t', cells));
                    }
                }
            }
        }

        var props = doc.PackageProperties;

        return new TikiSpreadsheet
        {
            Content = sb.Length > maxLength ? sb.ToString(0, maxLength) : sb.ToString(),
            MediaType = MediaType.Xlsx,
            Title = NullIfEmpty(props.Title),
            Authors = props.Creator != null ? new[] { props.Creator } : null,
            Description = NullIfEmpty(props.Description),
            Subject = NullIfEmpty(props.Subject),
            Keywords = ParseKeywords(props.Keywords),
            DateCreated = props.Created,
            DateModified = props.Modified,
            LastAuthor = NullIfEmpty(props.LastModifiedBy),
            Category = NullIfEmpty(props.Category),
            ContentLength = stream.CanSeek ? stream.Length : null
        };
    }

    private Documents.TikiFile ParsePresentation(Stream stream, int maxLength, CancellationToken cancellationToken)
    {
        using var doc = PresentationDocument.Open(stream, false);
        var sb = new StringBuilder();
        int slideCount = 0;

        var presentationPart = doc.PresentationPart;
        if (presentationPart?.Presentation?.SlideIdList != null)
        {
            foreach (var slideId in presentationPart.Presentation.SlideIdList.Elements<DocumentFormat.OpenXml.Presentation.SlideId>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (sb.Length >= maxLength) break;

                slideCount++;
                var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);
                var texts = slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>();

                foreach (var text in texts)
                {
                    if (sb.Length >= maxLength) break;
                    if (!string.IsNullOrWhiteSpace(text.Text))
                    {
                        if (sb.Length > 0) sb.Append('\n');
                        sb.Append(text.Text);
                    }
                }
            }
        }

        var props = doc.PackageProperties;

        return new TikiPresentation
        {
            Content = sb.Length > maxLength ? sb.ToString(0, maxLength) : sb.ToString(),
            MediaType = MediaType.Pptx,
            Title = NullIfEmpty(props.Title),
            Authors = props.Creator != null ? new[] { props.Creator } : null,
            Description = NullIfEmpty(props.Description),
            Subject = NullIfEmpty(props.Subject),
            Keywords = ParseKeywords(props.Keywords),
            DateCreated = props.Created,
            DateModified = props.Modified,
            LastAuthor = NullIfEmpty(props.LastModifiedBy),
            Category = NullIfEmpty(props.Category),
            SlideCount = slideCount > 0 ? slideCount : null,
            ContentLength = stream.CanSeek ? stream.Length : null
        };
    }

    private static string GetCellValue(Cell cell, SharedStringTable? sharedStrings)
    {
        var value = cell.CellValue?.Text ?? string.Empty;

        if (cell.DataType?.Value == CellValues.SharedString && sharedStrings != null)
        {
            if (int.TryParse(value, out var index))
            {
                var item = sharedStrings.ElementAtOrDefault(index);
                return item?.InnerText ?? value;
            }
        }

        return value;
    }

    private static bool TryOpenSpreadsheet(Stream stream)
    {
        try
        {
            using var _ = SpreadsheetDocument.Open(stream, false);
            return true;
        }
        catch { return false; }
    }

    private static bool TryOpenPresentation(Stream stream)
    {
        try
        {
            using var _ = PresentationDocument.Open(stream, false);
            return true;
        }
        catch { return false; }
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[]? ParseKeywords(string? keywords)
    {
        if (string.IsNullOrWhiteSpace(keywords)) return null;
        var result = keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return result.Length > 0 ? result : null;
    }
}
