using Tiki.Mime;

namespace Tiki.Detect;

/// <summary>
/// Detects media type by examining the magic bytes (file signature) at the start of a stream.
/// </summary>
public sealed class MagicBytesDetector : IDetector
{
    private static readonly List<MagicSignature> s_signatures = BuildSignatures();

    public async Task<MediaType> DetectAsync(Stream? stream, string? fileName = null, CancellationToken cancellationToken = default)
    {
        if (stream == null || !stream.CanRead)
            return MediaType.OctetStream;

        var buffer = new byte[16];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);

        if (bytesRead == 0)
            return MediaType.OctetStream;

        var span = buffer.AsSpan(0, bytesRead);

        foreach (var sig in s_signatures)
        {
            if (sig.Matches(span))
                return sig.MediaType;
        }

        // Check for text-based formats
        if (IsLikelyText(span))
        {
            // Read more to determine text subtype
            if (stream.CanSeek)
            {
                stream.Position = 0;
                var largerBuffer = new byte[512];
                bytesRead = await stream.ReadAsync(largerBuffer.AsMemory(), cancellationToken).ConfigureAwait(false);
                var text = System.Text.Encoding.UTF8.GetString(largerBuffer, 0, bytesRead);

                if (text.Contains("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("<html", StringComparison.OrdinalIgnoreCase))
                    return MediaType.TextHtml;

                if (text.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
                    text.TrimStart().StartsWith("<!", StringComparison.Ordinal))
                    return MediaType.TextXml;
            }

            return MediaType.TextPlain;
        }

        return MediaType.OctetStream;
    }

    private static bool IsLikelyText(ReadOnlySpan<byte> data)
    {
        // Check for UTF-8 BOM
        if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            return true;

        // Check if all bytes look like text (ASCII + common UTF-8)
        foreach (var b in data)
        {
            if (b == 0) return false; // Null byte means binary
            if (b < 0x09) return false; // Control chars below tab
            if (b > 0x0D && b < 0x20 && b != 0x1B) return false; // Control chars except ESC
        }
        return true;
    }

    private static List<MagicSignature> BuildSignatures()
    {
        return new List<MagicSignature>
        {
            // PDF
            new(new byte[] { 0x25, 0x50, 0x44, 0x46 }, MediaType.ApplicationPdf), // %PDF

            // ZIP-based (OOXML, EPUB, JAR, etc.) - detect ZIP first, refine later
            new(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, MediaType.ApplicationZip),

            // GZip
            new(new byte[] { 0x1F, 0x8B }, MediaType.ApplicationGzip),

            // PNG
            new(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, MediaType.ImagePng),

            // JPEG
            new(new byte[] { 0xFF, 0xD8, 0xFF }, MediaType.ImageJpeg),

            // GIF
            new(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, MediaType.ImageGif), // GIF87a
            new(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, MediaType.ImageGif), // GIF89a

            // BMP
            new(new byte[] { 0x42, 0x4D }, MediaType.ImageBmp),

            // TIFF
            new(new byte[] { 0x49, 0x49, 0x2A, 0x00 }, MediaType.ImageTiff), // Little-endian
            new(new byte[] { 0x4D, 0x4D, 0x00, 0x2A }, MediaType.ImageTiff), // Big-endian

            // WebP
            new(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 8, new byte[] { 0x57, 0x45, 0x42, 0x50 }, MediaType.ImageWebp), // RIFF....WEBP

            // MP3 (ID3 tag)
            new(new byte[] { 0x49, 0x44, 0x33 }, MediaType.AudioMpeg), // ID3
            new(new byte[] { 0xFF, 0xFB }, MediaType.AudioMpeg), // MPEG frame sync
            new(new byte[] { 0xFF, 0xF3 }, MediaType.AudioMpeg),
            new(new byte[] { 0xFF, 0xF2 }, MediaType.AudioMpeg),

            // FLAC
            new(new byte[] { 0x66, 0x4C, 0x61, 0x43 }, MediaType.AudioFlac), // fLaC

            // WAV
            new(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 8, new byte[] { 0x57, 0x41, 0x56, 0x45 }, MediaType.AudioWav), // RIFF....WAVE

            // OGG
            new(new byte[] { 0x4F, 0x67, 0x67, 0x53 }, MediaType.AudioOgg), // OggS

            // AVI
            new(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 8, new byte[] { 0x41, 0x56, 0x49, 0x20 }, MediaType.VideoAvi), // RIFF....AVI

            // MP4/MOV (ftyp box)
            new(4, new byte[] { 0x66, 0x74, 0x79, 0x70 }, MediaType.VideoMp4), // ....ftyp

            // MKV/WebM
            new(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, MediaType.VideoMkv), // EBML

            // RTF
            new(new byte[] { 0x7B, 0x5C, 0x72, 0x74, 0x66 }, MediaType.ApplicationRtf), // {\rtf

        };
    }

    private sealed class MagicSignature
    {
        private readonly byte[] _magic;
        private readonly int _offset;
        private readonly byte[]? _secondMagic;
        private readonly int _secondOffset;
        public MediaType MediaType { get; }

        public MagicSignature(byte[] magic, MediaType mediaType)
        {
            _magic = magic;
            _offset = 0;
            MediaType = mediaType;
        }

        public MagicSignature(int offset, byte[] magic, MediaType mediaType)
        {
            _magic = magic;
            _offset = offset;
            MediaType = mediaType;
        }

        public MagicSignature(byte[] magic, int secondOffset, byte[] secondMagic, MediaType mediaType)
        {
            _magic = magic;
            _offset = 0;
            _secondMagic = secondMagic;
            _secondOffset = secondOffset;
            MediaType = mediaType;
        }

        public bool Matches(ReadOnlySpan<byte> data)
        {
            if (data.Length < _offset + _magic.Length)
                return false;

            if (!data.Slice(_offset, _magic.Length).SequenceEqual(_magic))
                return false;

            if (_secondMagic != null)
            {
                if (data.Length < _secondOffset + _secondMagic.Length)
                    return false;
                return data.Slice(_secondOffset, _secondMagic.Length).SequenceEqual(_secondMagic);
            }

            return true;
        }
    }
}
