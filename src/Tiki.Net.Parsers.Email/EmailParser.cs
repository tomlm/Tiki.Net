using System.Text;
using System.Text.RegularExpressions;
using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;

namespace Tiki.Parsers.Email;

/// <summary>
/// Parser for RFC822 email messages (.eml files).
/// Extracts headers (From, To, CC, Subject, Date) and body text.
/// </summary>
public sealed partial class EmailParser : AbstractParser
{
    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.MessageRfc822
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override async Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        var maxLength = context?.MaxContentLength ?? int.MaxValue;

        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            // Parse headers and body
            var (headers, body) = SplitHeadersAndBody(content);

            var from = GetHeader(headers, "From");
            var to = GetHeader(headers, "To");
            var cc = GetHeader(headers, "Cc");
            var bcc = GetHeader(headers, "Bcc");
            var subject = GetHeader(headers, "Subject");
            var date = GetHeader(headers, "Date");
            var messageId = GetHeader(headers, "Message-ID") ?? GetHeader(headers, "Message-Id");

            // Parse email addresses
            var fromAddress = ExtractEmailAddress(from);
            var fromName = ExtractDisplayName(from);
            var toAddresses = ParseAddressList(to);
            var ccAddresses = ParseAddressList(cc);
            var bccAddresses = ParseAddressList(bcc);

            // Parse date
            DateTime? dateSent = null;
            if (!string.IsNullOrEmpty(date))
            {
                if (DateTimeOffset.TryParse(date, out var dto))
                    dateSent = dto.UtcDateTime;
            }

            // Handle body content
            var bodyText = DecodeBody(body, headers);
            if (bodyText.Length > maxLength)
                bodyText = bodyText[..maxLength];

            return new TikiMessage
            {
                Content = bodyText,
                MediaType = MediaType.MessageRfc822,
                Title = subject,
                Authors = fromAddress != null ? new[] { fromAddress } : null,
                DateCreated = dateSent,
                FromAddress = fromAddress,
                FromName = fromName,
                ToAddresses = toAddresses,
                CcAddresses = ccAddresses,
                BccAddresses = bccAddresses,
                Subject = subject,
                DateSent = dateSent,
                ConversationId = messageId?.Trim('<', '>'),
                ContentLength = stream.CanSeek ? stream.Length : null
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new ParseException($"Failed to parse email message: {ex.Message}", ex);
        }
    }

    private static (string headers, string body) SplitHeadersAndBody(string content)
    {
        // Headers and body are separated by a blank line (CRLFCRLF or LFLF)
        var separator = content.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (separator >= 0)
            return (content[..separator], content[(separator + 4)..]);

        separator = content.IndexOf("\n\n", StringComparison.Ordinal);
        if (separator >= 0)
            return (content[..separator], content[(separator + 2)..]);

        // No body
        return (content, string.Empty);
    }

    private static string? GetHeader(string headers, string name)
    {
        // Headers can be folded (continuation lines start with whitespace)
        var lines = headers.Split('\n');
        var prefix = name + ":";
        StringBuilder? value = null;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');

            if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = new StringBuilder(line[prefix.Length..].Trim());
            }
            else if (value != null && line.Length > 0 && (line[0] == ' ' || line[0] == '\t'))
            {
                // Continuation of previous header
                value.Append(' ').Append(line.Trim());
            }
            else if (value != null)
            {
                break;
            }
        }

        var result = value?.ToString();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    private static string? ExtractEmailAddress(string? headerValue)
    {
        if (string.IsNullOrEmpty(headerValue)) return null;

        // "Display Name <email@example.com>" or just "email@example.com"
        var match = AngleBracketEmail().Match(headerValue);
        if (match.Success)
            return match.Groups[1].Value;

        // Bare email
        if (headerValue.Contains('@'))
            return headerValue.Trim();

        return null;
    }

    private static string? ExtractDisplayName(string? headerValue)
    {
        if (string.IsNullOrEmpty(headerValue)) return null;

        // "Display Name <email@example.com>"
        var ltIndex = headerValue.IndexOf('<');
        if (ltIndex > 0)
        {
            var name = headerValue[..ltIndex].Trim().Trim('"');
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }

        return null;
    }

    private static string[]? ParseAddressList(string? headerValue)
    {
        if (string.IsNullOrEmpty(headerValue)) return null;

        var addresses = new List<string>();
        // Split on comma, but not inside angle brackets or quotes
        foreach (var part in SplitAddresses(headerValue))
        {
            var addr = ExtractEmailAddress(part.Trim());
            if (addr != null)
                addresses.Add(addr);
        }

        return addresses.Count > 0 ? addresses.ToArray() : null;
    }

    private static IEnumerable<string> SplitAddresses(string value)
    {
        int depth = 0;
        int start = 0;
        bool inQuotes = false;

        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '"') inQuotes = !inQuotes;
            else if (!inQuotes)
            {
                if (c == '<') depth++;
                else if (c == '>') depth--;
                else if (c == ',' && depth == 0)
                {
                    yield return value[start..i];
                    start = i + 1;
                }
            }
        }

        if (start < value.Length)
            yield return value[start..];
    }

    private static string DecodeBody(string body, string headers)
    {
        var contentType = GetHeader(headers, "Content-Type") ?? "";
        var transferEncoding = GetHeader(headers, "Content-Transfer-Encoding") ?? "";

        // If multipart, extract the text/plain part
        if (contentType.Contains("multipart", StringComparison.OrdinalIgnoreCase))
        {
            var boundary = ExtractBoundary(contentType);
            if (boundary != null)
            {
                var textPart = ExtractTextPart(body, boundary);
                if (textPart != null)
                    return textPart;
            }
        }

        // Decode transfer encoding
        if (transferEncoding.Contains("quoted-printable", StringComparison.OrdinalIgnoreCase))
            return DecodeQuotedPrintable(body);

        if (transferEncoding.Contains("base64", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var bytes = Convert.FromBase64String(body.Trim());
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return body;
            }
        }

        return body.Trim();
    }

    private static string? ExtractBoundary(string contentType)
    {
        var match = BoundaryPattern().Match(contentType);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string? ExtractTextPart(string body, string boundary)
    {
        var delimiter = "--" + boundary;
        var parts = body.Split(delimiter, StringSplitOptions.None);

        foreach (var part in parts)
        {
            if (part.StartsWith("--") || string.IsNullOrWhiteSpace(part))
                continue;

            var (partHeaders, partBody) = SplitHeadersAndBody(part.TrimStart('\r', '\n'));
            var partContentType = GetHeader(partHeaders, "Content-Type") ?? "";

            if (partContentType.Contains("text/plain", StringComparison.OrdinalIgnoreCase) ||
                (string.IsNullOrEmpty(partContentType) && !string.IsNullOrWhiteSpace(partBody)))
            {
                var partEncoding = GetHeader(partHeaders, "Content-Transfer-Encoding") ?? "";
                if (partEncoding.Contains("quoted-printable", StringComparison.OrdinalIgnoreCase))
                    return DecodeQuotedPrintable(partBody);
                if (partEncoding.Contains("base64", StringComparison.OrdinalIgnoreCase))
                {
                    try { return Encoding.UTF8.GetString(Convert.FromBase64String(partBody.Trim())); }
                    catch { return partBody.Trim(); }
                }
                return partBody.Trim();
            }
        }

        // Fall back to text/html stripped
        foreach (var part in parts)
        {
            if (part.StartsWith("--") || string.IsNullOrWhiteSpace(part))
                continue;

            var (partHeaders, partBody) = SplitHeadersAndBody(part.TrimStart('\r', '\n'));
            var partContentType = GetHeader(partHeaders, "Content-Type") ?? "";

            if (partContentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                var decoded = partBody;
                var partEncoding = GetHeader(partHeaders, "Content-Transfer-Encoding") ?? "";
                if (partEncoding.Contains("quoted-printable", StringComparison.OrdinalIgnoreCase))
                    decoded = DecodeQuotedPrintable(partBody);
                return StripHtml(decoded);
            }
        }

        return null;
    }

    private static string DecodeQuotedPrintable(string input)
    {
        var sb = new StringBuilder();
        var lines = input.Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r');

            // Soft line break
            if (line.EndsWith('='))
            {
                sb.Append(DecodeQpSegment(line[..^1]));
            }
            else
            {
                sb.Append(DecodeQpSegment(line));
                sb.Append('\n');
            }
        }

        return sb.ToString().Trim();
    }

    private static string DecodeQpSegment(string segment)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < segment.Length; i++)
        {
            if (segment[i] == '=' && i + 2 < segment.Length)
            {
                var hex = segment.Substring(i + 1, 2);
                if (byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var b))
                {
                    sb.Append((char)b);
                    i += 2;
                }
                else
                {
                    sb.Append(segment[i]);
                }
            }
            else
            {
                sb.Append(segment[i]);
            }
        }
        return sb.ToString();
    }

    private static string StripHtml(string html)
    {
        var sb = new StringBuilder();
        bool inTag = false;
        foreach (var c in html)
        {
            if (c == '<') { inTag = true; continue; }
            if (c == '>') { inTag = false; continue; }
            if (!inTag) sb.Append(c);
        }
        return sb.ToString().Trim();
    }

    [GeneratedRegex(@"<([^>]+)>")]
    private static partial Regex AngleBracketEmail();

    [GeneratedRegex(@"boundary=""?([^"";\s]+)""?", RegexOptions.IgnoreCase)]
    private static partial Regex BoundaryPattern();
}
