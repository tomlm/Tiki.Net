using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;

namespace Tiki.Parsers.Html;

/// <summary>
/// Parser for HTML documents using AngleSharp.
/// </summary>
public sealed class HtmlParser : AbstractParser
{
    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.TextHtml
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override async Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        var maxLength = context?.MaxContentLength ?? int.MaxValue;

        try
        {
            var config = Configuration.Default;
            var browsingContext = BrowsingContext.New(config);
            var parser = new AngleSharp.Html.Parser.HtmlParser(new HtmlParserOptions(), browsingContext);

            var document = await parser.ParseDocumentAsync(stream, cancellationToken).ConfigureAwait(false);

            // Extract metadata
            var title = document.Title;
            var description = GetMetaContent(document, "description");
            var author = GetMetaContent(document, "author");
            var keywords = GetMetaContent(document, "keywords");
            var generator = GetMetaContent(document, "generator");
            var charset = document.CharacterSet;
            var language = document.DocumentElement?.GetAttribute("lang")
                ?? GetMetaContent(document, "language");

            // Extract links
            var links = document.QuerySelectorAll("a[href]")
                .Select(a => a.GetAttribute("href"))
                .Where(href => !string.IsNullOrEmpty(href))
                .Cast<string>()
                .Distinct()
                .ToArray();

            // Extract text content from body
            var body = document.Body;
            var content = body != null ? ExtractText(body, maxLength) : string.Empty;

            // Parse keywords
            string[]? keywordList = null;
            if (!string.IsNullOrWhiteSpace(keywords))
                keywordList = keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return new TikiWebPage
            {
                Content = content,
                MediaType = MediaType.TextHtml,
                Title = NullIfEmpty(title),
                Authors = author != null ? new[] { author } : null,
                Description = NullIfEmpty(description),
                Keywords = keywordList,
                Language = NullIfEmpty(language),
                Generator = NullIfEmpty(generator),
                Charset = NullIfEmpty(charset),
                Links = links.Length > 0 ? links : null,
                ContentLength = stream.CanSeek ? stream.Length : null
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new ParseException($"Failed to parse HTML document: {ex.Message}", ex);
        }
    }

    private static string ExtractText(INode node, int maxLength)
    {
        var sb = new StringBuilder();
        ExtractTextRecursive(node, sb, maxLength);
        return sb.ToString().Trim();
    }

    private static void ExtractTextRecursive(INode node, StringBuilder sb, int maxLength)
    {
        if (sb.Length >= maxLength) return;

        if (node is IText textNode)
        {
            var text = textNode.Data;
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (sb.Length > 0 && sb[^1] != '\n' && sb[^1] != ' ')
                    sb.Append(' ');
                sb.Append(text.Trim());
            }
            return;
        }

        if (node is IElement element)
        {
            // Skip script and style elements
            var tagName = element.TagName.ToLowerInvariant();
            if (tagName is "script" or "style" or "noscript")
                return;

            var isBlock = tagName is "p" or "div" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6"
                or "li" or "br" or "hr" or "tr" or "blockquote" or "pre" or "section" or "article";

            if (isBlock && sb.Length > 0 && sb[^1] != '\n')
                sb.Append('\n');

            foreach (var child in node.ChildNodes)
            {
                if (sb.Length >= maxLength) break;
                ExtractTextRecursive(child, sb, maxLength);
            }

            if (isBlock && sb.Length > 0 && sb[^1] != '\n')
                sb.Append('\n');
        }
        else
        {
            foreach (var child in node.ChildNodes)
            {
                if (sb.Length >= maxLength) break;
                ExtractTextRecursive(child, sb, maxLength);
            }
        }
    }

    private static string? GetMetaContent(IDocument document, string name)
    {
        var meta = document.QuerySelector($"meta[name='{name}']")
            ?? document.QuerySelector($"meta[property='{name}']");
        return NullIfEmpty(meta?.GetAttribute("content"));
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
