namespace Tiki.Parsers.Rtf;

/// <summary>
/// Extension methods for adding RTF parser to TikiConfig.
/// </summary>
public static class RtfTikiExtensions
{
    public static TikiConfig.Builder AddRtfParser(this TikiConfig.Builder builder)
    {
        builder.AddParser(new RtfParser());
        return builder;
    }
}
