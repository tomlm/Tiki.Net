using Tiki.Mime;

namespace Tiki.Detect;

/// <summary>
/// The detector used when none is configured. Like Apache Tika's DefaultDetector it reads the
/// content first and treats the file name as a hint: a name decides only when the content
/// cannot, or when it narrows down what the content was found to be.
/// </summary>
internal sealed class DefaultDetector : IDetector
{
    // How much of the stream is checked before content that "looks like text" may overrule a name.
    private const int TextSampleSize = 4096;

    // What MagicBytesDetector answers for content that looks like text. It can tell that much
    // from the bytes, but not which text format it is.
    private static readonly HashSet<MediaType> s_textContent = new()
    {
        MediaType.TextPlain,
        MediaType.TextHtml,
        MediaType.TextXml
    };

    // The formats ExtensionDetector knows that are stored as text.
    private static readonly HashSet<MediaType> s_textFormats = new()
    {
        MediaType.TextPlain,
        MediaType.Parse("text/csv"),
        MediaType.Parse("text/tab-separated-values"),
        MediaType.ApplicationJson,
        MediaType.TextXml,
        MediaType.TextHtml,
        MediaType.ImageSvg,
        MediaType.MessageRfc822
    };

    // Signatures that identify a container shared by several formats, and the formats a name
    // may narrow each one down to.
    private static readonly Dictionary<MediaType, MediaType[]> s_containers = new()
    {
        // ZIP holds OOXML and OpenDocument
        [MediaType.ApplicationZip] = new[]
        {
            MediaType.Docx, MediaType.Xlsx, MediaType.Pptx,
            MediaType.Odt, MediaType.Ods, MediaType.Odp
        },

        // The "ftyp" box opens M4A and QuickTime files as well as MP4
        [MediaType.VideoMp4] = new[] { MediaType.AudioMp4, MediaType.VideoMov },

        // EBML opens WebM as well as Matroska
        [MediaType.VideoMkv] = new[] { MediaType.VideoWebm },

        // An ID3v2 tag can precede FLAC and AAC streams as well as MP3. The audio after the tag decides
        // when it can be read (see AudioAfterTagAsync); this applies only when it cannot.
        [MediaType.AudioMpeg] = new[] { MediaType.AudioFlac, MediaType.AudioMp4 }
    };

    private readonly MagicBytesDetector _contentDetector = new();
    private readonly ExtensionDetector _nameDetector = new();

    public async Task<MediaType> DetectAsync(Stream? stream, string? fileName = null, CancellationToken cancellationToken = default)
    {
        var seekable = stream is { CanSeek: true } ? stream : null;
        var position = seekable?.Position ?? 0;

        try
        {
            var byName = await _nameDetector.DetectAsync(null, fileName, cancellationToken).ConfigureAwait(false);
            var byContent = await _contentDetector.DetectAsync(stream, fileName, cancellationToken).ConfigureAwait(false);

            // The MP3 signatures include the ID3v2 tag, which FLAC and AAC streams can open with too.
            // Where the audio itself can be identified, it wins over the name like any other signature.
            if (byContent == MediaType.AudioMpeg &&
                await AudioAfterTagAsync(seekable, position, cancellationToken).ConfigureAwait(false) is { } audio)
                return audio;

            // Only one of them has an answer, or they agree
            if (byContent == MediaType.OctetStream)
                return byName;
            if (byName == MediaType.OctetStream || byName == byContent)
                return byContent;

            if (s_textContent.Contains(byContent))
            {
                // Text that opens as an HTML document is HTML whatever the name says. MagicBytesDetector also
                // answers text/html when "<html" merely appears near the start (an e-mail's headers, source
                // code), and there the name should still decide.
                if (byContent == MediaType.TextHtml && await StartsAsHtmlAsync(seekable, position, cancellationToken).ConfigureAwait(false))
                    return byContent;

                // The content is some kind of text, and the name says which (CSV, JSON, markup, source code)
                if (s_textFormats.Contains(byName))
                    return byName;

                // The name says the content is binary. "Looks like text" rests on the first 16 bytes,
                // which compressed data with no signature (ADTS AAC, an MP3 with no tag) can pass too,
                // so only believe it over the name when a larger sample is text as well.
                return await IsTextAsync(seekable, position, cancellationToken).ConfigureAwait(false)
                    ? byContent
                    : byName;
            }

            // The content has a signature. A name that disagrees with it is wrong, unless the signature
            // is a container's and the name picks one of the formats that container holds.
            return s_containers.TryGetValue(byContent, out var formats) && formats.Contains(byName)
                ? byName
                : byContent;
        }
        finally
        {
            // Leave the stream where it was, so it can be parsed after being detected
            if (seekable != null)
                seekable.Position = position;
        }
    }

    // Identifies the audio that content with an MP3 signature holds: what follows the ID3v2 tag, or the
    // MPEG frame the content opens with. Null when that cannot be told: the stream cannot seek, the tag
    // runs past the end, or the tag is followed by something else (ADTS AAC, which Tiki types as
    // audio/mp4 along with M4A, so only the name can say which TagLib reader fits; or padding).
    private static async Task<MediaType?> AudioAfterTagAsync(Stream? stream, long position, CancellationToken cancellationToken)
    {
        if (stream == null)
            return null;

        stream.Position = position;
        var header = new byte[10];
        if (await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false) < 4)
            return null;

        var start = position;
        if (header[0] == 'I' && header[1] == 'D' && header[2] == '3')
        {
            // Bytes 6-9 hold the tag size as a syncsafe integer (7 bits a byte), not counting the 10-byte
            // header, or the 10-byte footer that flag bit 4 of byte 5 adds
            var size = (header[6] & 0x7F) << 21 | (header[7] & 0x7F) << 14 | (header[8] & 0x7F) << 7 | (header[9] & 0x7F);
            start += 10 + size + ((header[5] & 0x10) != 0 ? 10 : 0);

            stream.Position = start;
            if (await stream.ReadAtLeastAsync(header.AsMemory(0, 4), 4, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false) < 4)
                return null;
        }

        if (header[0] == 'f' && header[1] == 'L' && header[2] == 'a' && header[3] == 'C')
            return MediaType.AudioFlac;

        // An MPEG audio frame: 11 sync bits, then a version and a layer. ADTS has the same sync and layer 0.
        if (header[0] == 0xFF && (header[1] & 0xE0) == 0xE0 && (header[1] & 0x06) != 0)
            return MediaType.AudioMpeg;

        return null;
    }

    private static async Task<bool> IsTextAsync(Stream? stream, long position, CancellationToken cancellationToken)
    {
        // Without seeking there is no way to look again, so the name stands
        if (stream == null)
            return false;

        stream.Position = position;
        var buffer = new byte[TextSampleSize];
        var bytesRead = await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false);

        // The control characters MagicBytesDetector treats as binary
        foreach (var b in buffer.AsSpan(0, bytesRead))
        {
            if (b < 0x09) return false;
            if (b > 0x0D && b < 0x20 && b != 0x1B) return false;
        }

        return bytesRead > 0;
    }

    private static async Task<bool> StartsAsHtmlAsync(Stream? stream, long position, CancellationToken cancellationToken)
    {
        if (stream == null)
            return false;

        stream.Position = position;
        var buffer = new byte[512];
        var bytesRead = await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false);
        var text = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimStart('﻿', ' ', '\t', '\r', '\n');

        return text.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
    }
}
