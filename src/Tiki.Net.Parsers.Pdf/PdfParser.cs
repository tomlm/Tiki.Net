using System.Text;
using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;
using UglyToad.PdfPig;

namespace Tiki.Parsers.Pdf;

/// <summary>
/// Parser for PDF documents using PdfPig.
/// </summary>
public sealed class PdfParser : AbstractParser
{
    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.ApplicationPdf
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        var maxLength = context?.MaxContentLength ?? int.MaxValue;

        try
        {
            using var document = PdfDocument.Open(stream);

            var sb = new StringBuilder();
            string? title = null;
            string? author = null;
            DateTime? createdDate = null;
            DateTime? modifiedDate = null;
            string? subject = null;
            string[]? keywords = null;
            string? creator = null;

            // Extract metadata from document info
            var info = document.Information;
            if (info != null)
            {
                title = NullIfEmpty(info.Title);
                author = NullIfEmpty(info.Author);
                subject = NullIfEmpty(info.Subject);
                creator = NullIfEmpty(info.Creator);

                if (!string.IsNullOrWhiteSpace(info.Keywords))
                    keywords = info.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                // DocumentInformation exposes dates via the DocumentInformationDictionary
                // Try to get dates from the dictionary entries
                try
                {
                    var dict = info.DocumentInformationDictionary;
                    if (dict.TryGet(UglyToad.PdfPig.Tokens.NameToken.Create("CreationDate"), out var creationToken))
                        createdDate = ParsePdfDate(creationToken?.ToString());
                    if (dict.TryGet(UglyToad.PdfPig.Tokens.NameToken.Create("ModDate"), out var modToken))
                        modifiedDate = ParsePdfDate(modToken?.ToString());
                }
                catch
                {
                    // Date extraction is best-effort
                }
            }

            // Extract text from all pages
            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (sb.Length >= maxLength) break;

                var pageText = page.Text;
                if (!string.IsNullOrEmpty(pageText))
                {
                    if (sb.Length > 0) sb.Append('\n');
                    sb.Append(pageText);
                }
            }

            var content = sb.Length > maxLength ? sb.ToString(0, maxLength) : sb.ToString();

            return Task.FromResult<Documents.TikiFile>(new TikiDocument
            {
                Content = content,
                MediaType = MediaType.ApplicationPdf,
                Title = title,
                Authors = author != null ? new[] { author } : null,
                DateCreated = createdDate,
                DateModified = modifiedDate,
                Description = subject,
                Keywords = keywords,
                PageCount = document.NumberOfPages,
                ApplicationName = creator,
                ContentLength = stream.CanSeek ? stream.Length : null
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("encrypt", StringComparison.OrdinalIgnoreCase))
            {
                throw new EncryptedDocumentException("The PDF document is encrypted or password-protected.", ex);
            }
            throw new ParseException($"Failed to parse PDF document: {ex.Message}", ex);
        }
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime? ParsePdfDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        // PDF dates are formatted as D:YYYYMMDDHHmmSSOHH'mm'
        var s = dateString.Trim();
        if (s.StartsWith("D:"))
            s = s[2..];

        if (DateTime.TryParse(s, out var result))
            return result;

        // Try parsing PDF date format manually
        if (s.Length >= 8 &&
            int.TryParse(s[..4], out var year) &&
            int.TryParse(s[4..6], out var month) &&
            int.TryParse(s[6..8], out var day))
        {
            var hour = s.Length >= 10 && int.TryParse(s[8..10], out var h) ? h : 0;
            var minute = s.Length >= 12 && int.TryParse(s[10..12], out var m) ? m : 0;
            var second = s.Length >= 14 && int.TryParse(s[12..14], out var sec) ? sec : 0;

            try
            {
                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}
