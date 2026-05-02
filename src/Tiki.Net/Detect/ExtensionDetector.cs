using Tiki.Mime;

namespace Tiki.Detect;

/// <summary>
/// Detects media type based on file extension.
/// </summary>
public sealed class ExtensionDetector : IDetector
{
    private static readonly Dictionary<string, MediaType> s_extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Text
        [".txt"] = MediaType.TextPlain,
        [".text"] = MediaType.TextPlain,
        [".log"] = MediaType.TextPlain,
        [".csv"] = MediaType.Parse("text/csv"),
        [".tsv"] = MediaType.Parse("text/tab-separated-values"),
        [".md"] = MediaType.TextPlain,
        [".markdown"] = MediaType.TextPlain,
        [".ini"] = MediaType.TextPlain,
        [".cfg"] = MediaType.TextPlain,
        [".conf"] = MediaType.TextPlain,
        [".yaml"] = MediaType.TextPlain,
        [".yml"] = MediaType.TextPlain,
        [".toml"] = MediaType.TextPlain,

        // Source code
        [".cs"] = MediaType.TextPlain,
        [".cpp"] = MediaType.TextPlain,
        [".c"] = MediaType.TextPlain,
        [".h"] = MediaType.TextPlain,
        [".hpp"] = MediaType.TextPlain,
        [".java"] = MediaType.TextPlain,
        [".py"] = MediaType.TextPlain,
        [".js"] = MediaType.TextPlain,
        [".ts"] = MediaType.TextPlain,
        [".jsx"] = MediaType.TextPlain,
        [".tsx"] = MediaType.TextPlain,
        [".go"] = MediaType.TextPlain,
        [".rs"] = MediaType.TextPlain,
        [".rb"] = MediaType.TextPlain,
        [".swift"] = MediaType.TextPlain,
        [".kt"] = MediaType.TextPlain,
        [".scala"] = MediaType.TextPlain,
        [".php"] = MediaType.TextPlain,
        [".sh"] = MediaType.TextPlain,
        [".bash"] = MediaType.TextPlain,
        [".ps1"] = MediaType.TextPlain,
        [".bat"] = MediaType.TextPlain,
        [".cmd"] = MediaType.TextPlain,
        [".sql"] = MediaType.TextPlain,
        [".r"] = MediaType.TextPlain,
        [".m"] = MediaType.TextPlain,
        [".fs"] = MediaType.TextPlain,
        [".fsx"] = MediaType.TextPlain,
        [".vb"] = MediaType.TextPlain,
        [".lua"] = MediaType.TextPlain,
        [".pl"] = MediaType.TextPlain,
        [".dart"] = MediaType.TextPlain,
        [".zig"] = MediaType.TextPlain,

        // XML
        [".xml"] = MediaType.TextXml,
        [".xaml"] = MediaType.TextXml,
        [".xsl"] = MediaType.TextXml,
        [".xslt"] = MediaType.TextXml,
        [".svg"] = MediaType.ImageSvg,

        // HTML
        [".html"] = MediaType.TextHtml,
        [".htm"] = MediaType.TextHtml,
        [".xhtml"] = MediaType.TextHtml,

        // JSON
        [".json"] = MediaType.ApplicationJson,

        // PDF
        [".pdf"] = MediaType.ApplicationPdf,

        // RTF
        [".rtf"] = MediaType.ApplicationRtf,

        // OpenDocument
        [".odt"] = MediaType.Odt,
        [".ods"] = MediaType.Ods,
        [".odp"] = MediaType.Odp,

        // Office OOXML
        [".docx"] = MediaType.Docx,
        [".xlsx"] = MediaType.Xlsx,
        [".pptx"] = MediaType.Pptx,


        // Images
        [".jpg"] = MediaType.ImageJpeg,
        [".jpeg"] = MediaType.ImageJpeg,
        [".png"] = MediaType.ImagePng,
        [".gif"] = MediaType.ImageGif,
        [".tiff"] = MediaType.ImageTiff,
        [".tif"] = MediaType.ImageTiff,
        [".bmp"] = MediaType.ImageBmp,
        [".webp"] = MediaType.ImageWebp,

        // Audio
        [".mp3"] = MediaType.AudioMpeg,
        [".flac"] = MediaType.AudioFlac,
        [".wav"] = MediaType.AudioWav,
        [".ogg"] = MediaType.AudioOgg,
        [".m4a"] = MediaType.AudioMp4,
        [".aac"] = MediaType.AudioMp4,

        // Video
        [".mp4"] = MediaType.VideoMp4,
        [".m4v"] = MediaType.VideoMp4,
        [".avi"] = MediaType.VideoAvi,
        [".mkv"] = MediaType.VideoMkv,
        [".mov"] = MediaType.VideoMov,
        [".webm"] = MediaType.VideoWebm,
        [".wmv"] = MediaType.VideoWmv,

        // Archives
        [".zip"] = MediaType.ApplicationZip,
        [".gz"] = MediaType.ApplicationGzip,
        [".gzip"] = MediaType.ApplicationGzip,

        // Email
        [".eml"] = MediaType.MessageRfc822,
        [".msg"] = MediaType.ApplicationMsOutlookMsg,
    };

    public Task<MediaType> DetectAsync(Stream? stream, string? fileName = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileName))
            return Task.FromResult(MediaType.OctetStream);

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
            return Task.FromResult(MediaType.OctetStream);

        var result = s_extensions.GetValueOrDefault(extension, MediaType.OctetStream);
        return Task.FromResult(result);
    }
}
