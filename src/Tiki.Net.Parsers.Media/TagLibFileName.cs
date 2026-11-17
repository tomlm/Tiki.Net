using Tiki.Mime;
using Tiki.Parser;

namespace Tiki.Parsers.Media;

/// <summary>
/// Chooses the file name handed to TagLib, which picks its reader from the name's extension alone.
/// </summary>
internal static class TagLibFileName
{
    public static string Resolve(ParseContext? context, IReadOnlyDictionary<MediaType, string[]> extensions, string fallback)
    {
        var fileName = string.IsNullOrWhiteSpace(context?.FileName) ? null : context.FileName;

        // Called without detection (directly, not through AutoDetectParser): the name is all there is
        var detected = context?.Get<MediaType>();
        if (detected == null || !extensions.TryGetValue(detected, out var known))
            return fileName ?? fallback;

        // A name that TagLib reads as the detected type is kept. Anything else ("voicemail.dat", no
        // extension, an extension for some other format) would make TagLib pick the wrong reader or
        // none, so name the file after what was detected instead.
        if (fileName != null && known.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase))
            return fileName;

        return Path.ChangeExtension(fallback, known[0]);
    }
}
