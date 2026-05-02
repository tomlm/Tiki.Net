using System.IO.Compression;
using System.Text;
using System.Xml;
using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;

namespace Tiki.Parsers.Office;

/// <summary>
/// Parser for OpenDocument Text files (ODT, ODS, ODP).
/// These are ZIP archives containing XML content.
/// </summary>
public sealed class OdtParser : AbstractParser
{
    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.Odt,
        MediaType.Ods,
        MediaType.Odp
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override Task<TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        var maxLength = context?.MaxContentLength ?? int.MaxValue;
        var fileName = context?.FileName;
        var extension = fileName != null ? Path.GetExtension(fileName)?.ToLowerInvariant() : null;

        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

            // Extract text from content.xml
            var contentEntry = archive.GetEntry("content.xml");
            var content = string.Empty;

            if (contentEntry != null)
            {
                using var entryStream = contentEntry.Open();
                content = ExtractTextFromOdfXml(entryStream, maxLength, cancellationToken);
            }

            // Extract metadata from meta.xml
            string? title = null;
            string? description = null;
            string? subject = null;
            string? creator = null;
            DateTime? dateCreated = null;
            DateTime? dateModified = null;
            string[]? keywords = null;

            var metaEntry = archive.GetEntry("meta.xml");
            if (metaEntry != null)
            {
                using var metaStream = metaEntry.Open();
                (title, description, subject, creator, dateCreated, dateModified, keywords) = ExtractMetadata(metaStream);
            }

            var mediaType = extension switch
            {
                ".ods" => MediaType.Ods,
                ".odp" => MediaType.Odp,
                _ => MediaType.Odt
            };

            return Task.FromResult<TikiFile>(new TikiDocument
            {
                Content = content,
                MediaType = mediaType,
                Title = title,
                Authors = creator != null ? new[] { creator } : null,
                Description = description,
                Subject = subject,
                Keywords = keywords,
                DateCreated = dateCreated,
                DateModified = dateModified,
                ContentLength = stream.CanSeek ? stream.Length : null
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new ParseException($"Failed to parse OpenDocument file: {ex.Message}", ex);
        }
    }

    private static string ExtractTextFromOdfXml(Stream stream, int maxLength, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null
        };

        using var reader = XmlReader.Create(stream, settings);
        bool inText = false;

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (sb.Length >= maxLength) break;

            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    // ODF text elements: text:p, text:h, text:span, text:s, text:tab, text:line-break
                    if (reader.LocalName is "p" or "h" && reader.NamespaceURI.Contains("text"))
                    {
                        if (sb.Length > 0 && sb[^1] != '\n')
                            sb.Append('\n');
                        inText = true;
                    }
                    else if (reader.LocalName == "tab" && reader.NamespaceURI.Contains("text"))
                    {
                        sb.Append('\t');
                    }
                    else if (reader.LocalName == "line-break" && reader.NamespaceURI.Contains("text"))
                    {
                        sb.Append('\n');
                    }
                    break;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    if (inText)
                        sb.Append(reader.Value);
                    break;

                case XmlNodeType.EndElement:
                    if (reader.LocalName is "p" or "h" && reader.NamespaceURI.Contains("text"))
                        inText = false;
                    break;
            }
        }

        return sb.Length > maxLength ? sb.ToString(0, maxLength) : sb.ToString();
    }

    private static (string? title, string? description, string? subject, string? creator, DateTime? created, DateTime? modified, string[]? keywords) ExtractMetadata(Stream stream)
    {
        string? title = null;
        string? description = null;
        string? subject = null;
        string? creator = null;
        DateTime? created = null;
        DateTime? modified = null;
        List<string>? keywords = null;

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null
        };

        using var reader = XmlReader.Create(stream, settings);
        string? currentElement = null;

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    currentElement = reader.LocalName;
                    break;

                case XmlNodeType.Text:
                    switch (currentElement)
                    {
                        case "title": title = reader.Value; break;
                        case "description": description = reader.Value; break;
                        case "subject": subject = reader.Value; break;
                        case "initial-creator" or "creator": creator ??= reader.Value; break;
                        case "creation-date":
                            if (DateTime.TryParse(reader.Value, out var c)) created = c;
                            break;
                        case "date":
                            if (DateTime.TryParse(reader.Value, out var m)) modified = m;
                            break;
                        case "keyword":
                            keywords ??= new List<string>();
                            keywords.Add(reader.Value);
                            break;
                    }
                    break;

                case XmlNodeType.EndElement:
                    currentElement = null;
                    break;
            }
        }

        return (title, description, subject, creator, created, modified, keywords?.ToArray());
    }
}
