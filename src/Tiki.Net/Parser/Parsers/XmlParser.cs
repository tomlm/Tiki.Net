using System.Text;
using System.Xml;
using Tiki.Documents;
using Tiki.Mime;

namespace Tiki.Parser.Parsers;

/// <summary>
/// Parser for XML documents. Extracts text content from all elements.
/// </summary>
public sealed class XmlParser : AbstractParser
{
    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.TextXml,
        MediaType.ApplicationXml,
        MediaType.ImageSvg
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override async Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        var maxLength = context?.MaxContentLength ?? int.MaxValue;
        var sb = new StringBuilder();
        string? title = null;

        var settings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null
        };

        using var reader = XmlReader.Create(stream, settings);

        try
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (sb.Length >= maxLength) break;

                if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
                {
                    var value = reader.Value;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (sb.Length > 0) sb.Append(' ');
                        sb.Append(value.Trim());
                    }
                }
            }
        }
        catch (XmlException)
        {
            // Return what we have so far for malformed XML
        }

        return new Documents.TikiData
        {
            Content = sb.Length > maxLength ? sb.ToString(0, maxLength) : sb.ToString(),
            MediaType = MediaType.TextXml,
            Title = title,
            ContentLength = stream.CanSeek ? stream.Length : null
        };
    }
}
