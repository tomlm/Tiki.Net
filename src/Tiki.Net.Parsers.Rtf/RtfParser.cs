using System.Text;
using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;

namespace Tiki.Parsers.Rtf;

/// <summary>
/// Parser for RTF documents using RtfPipe.
/// </summary>
public sealed class RtfParser : AbstractParser
{
    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.ApplicationRtf
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        var maxLength = context?.MaxContentLength ?? int.MaxValue;

        try
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            var rtfContent = reader.ReadToEnd();

            // Convert RTF to HTML, then strip tags for plain text
            var html = RtfPipe.Rtf.ToHtml(rtfContent);
            var text = StripHtml(html);

            var content = text.Length > maxLength ? text[..maxLength] : text;

            return Task.FromResult<Documents.TikiFile>(new Documents.TikiDocument
            {
                Content = content,
                MediaType = MediaType.ApplicationRtf,
                ContentLength = stream.CanSeek ? stream.Length : null
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new ParseException($"Failed to parse RTF document: {ex.Message}", ex);
        }
    }

    private static string StripHtml(string html)
    {
        var sb = new StringBuilder();
        bool inTag = false;
        bool lastWasSpace = false;

        foreach (var c in html)
        {
            if (c == '<') { inTag = true; continue; }
            if (c == '>')
            {
                inTag = false;
                if (!lastWasSpace && sb.Length > 0)
                {
                    sb.Append(' ');
                    lastWasSpace = true;
                }
                continue;
            }

            if (!inTag)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasSpace && sb.Length > 0)
                    {
                        sb.Append(' ');
                        lastWasSpace = true;
                    }
                }
                else
                {
                    sb.Append(c);
                    lastWasSpace = false;
                }
            }
        }

        return sb.ToString().Trim();
    }
}
