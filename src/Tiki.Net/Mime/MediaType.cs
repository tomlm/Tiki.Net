namespace Tiki.Mime;

/// <summary>
/// Represents a MIME media type (e.g., "application/pdf").
/// </summary>
public sealed record MediaType(string Type, string Subtype, IReadOnlyDictionary<string, string>? Parameters = null)
{
    public static readonly MediaType OctetStream = new("application", "octet-stream");
    public static readonly MediaType TextPlain = new("text", "plain");
    public static readonly MediaType TextHtml = new("text", "html");
    public static readonly MediaType TextXml = new("text", "xml");
    public static readonly MediaType ApplicationXml = new("application", "xml");
    public static readonly MediaType ApplicationJson = new("application", "json");
    public static readonly MediaType ApplicationPdf = new("application", "pdf");
    public static readonly MediaType ApplicationRtf = new("application", "rtf");
    public static readonly MediaType ApplicationZip = new("application", "zip");
    public static readonly MediaType ApplicationGzip = new("application", "gzip");

    // OpenDocument
    public static readonly MediaType Odt = new("application", "vnd.oasis.opendocument.text");
    public static readonly MediaType Ods = new("application", "vnd.oasis.opendocument.spreadsheet");
    public static readonly MediaType Odp = new("application", "vnd.oasis.opendocument.presentation");

    // Office OOXML
    public static readonly MediaType Docx = new("application", "vnd.openxmlformats-officedocument.wordprocessingml.document");
    public static readonly MediaType Xlsx = new("application", "vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    public static readonly MediaType Pptx = new("application", "vnd.openxmlformats-officedocument.presentationml.presentation");


    // Images
    public static readonly MediaType ImageJpeg = new("image", "jpeg");
    public static readonly MediaType ImagePng = new("image", "png");
    public static readonly MediaType ImageGif = new("image", "gif");
    public static readonly MediaType ImageTiff = new("image", "tiff");
    public static readonly MediaType ImageBmp = new("image", "bmp");
    public static readonly MediaType ImageWebp = new("image", "webp");
    public static readonly MediaType ImageSvg = new("image", "svg+xml");

    // Audio
    public static readonly MediaType AudioMpeg = new("audio", "mpeg");
    public static readonly MediaType AudioFlac = new("audio", "flac");
    public static readonly MediaType AudioWav = new("audio", "wav");
    public static readonly MediaType AudioOgg = new("audio", "ogg");
    public static readonly MediaType AudioMp4 = new("audio", "mp4");

    // Video
    public static readonly MediaType VideoMp4 = new("video", "mp4");
    public static readonly MediaType VideoAvi = new("video", "x-msvideo");
    public static readonly MediaType VideoMkv = new("video", "x-matroska");
    public static readonly MediaType VideoMov = new("video", "quicktime");
    public static readonly MediaType VideoWebm = new("video", "webm");
    public static readonly MediaType VideoWmv = new("video", "x-ms-wmv");

    // Email
    public static readonly MediaType MessageRfc822 = new("message", "rfc822");
    public static readonly MediaType ApplicationMsOutlookMsg = new("application", "vnd.ms-outlook");

    public static MediaType Parse(string mimeType)
    {
        ArgumentNullException.ThrowIfNull(mimeType);

        var paramIndex = mimeType.IndexOf(';');
        var baseType = paramIndex >= 0 ? mimeType[..paramIndex].Trim() : mimeType.Trim();

        var slashIndex = baseType.IndexOf('/');
        if (slashIndex < 0)
            throw new FormatException($"Invalid media type: '{mimeType}'");

        var type = baseType[..slashIndex].ToLowerInvariant();
        var subtype = baseType[(slashIndex + 1)..].ToLowerInvariant();

        Dictionary<string, string>? parameters = null;
        if (paramIndex >= 0)
        {
            parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var paramString = mimeType[(paramIndex + 1)..];
            foreach (var param in paramString.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var eqIndex = param.IndexOf('=');
                if (eqIndex > 0)
                {
                    var key = param[..eqIndex].Trim().ToLowerInvariant();
                    var value = param[(eqIndex + 1)..].Trim().Trim('"');
                    parameters[key] = value;
                }
            }
        }

        return new MediaType(type, subtype, parameters);
    }

    public bool Equals(MediaType? other)
    {
        if (other is null) return false;
        return string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Subtype, other.Subtype, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
        => HashCode.Combine(Type.ToLowerInvariant(), Subtype.ToLowerInvariant());

    public override string ToString()
    {
        if (Parameters is null or { Count: 0 })
            return $"{Type}/{Subtype}";
        return $"{Type}/{Subtype}; {string.Join("; ", Parameters.Select(p => $"{p.Key}={p.Value}"))}";
    }
}
