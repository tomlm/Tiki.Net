namespace Tiki.Parsers.Media;

/// <summary>
/// Extension methods for adding audio and video parsers to TikiConfig.
/// </summary>
public static class MediaTikiExtensions
{
    /// <summary>
    /// Adds both audio and video parsers.
    /// </summary>
    public static TikiConfig.Builder AddMediaParser(this TikiConfig.Builder builder)
    {
        builder.AddParser(new AudioParser());
        builder.AddParser(new VideoParser());
        return builder;
    }

    /// <summary>
    /// Adds only the audio parser.
    /// </summary>
    public static TikiConfig.Builder AddAudioParser(this TikiConfig.Builder builder)
    {
        builder.AddParser(new AudioParser());
        return builder;
    }

    /// <summary>
    /// Adds only the video parser.
    /// </summary>
    public static TikiConfig.Builder AddVideoParser(this TikiConfig.Builder builder)
    {
        builder.AddParser(new VideoParser());
        return builder;
    }
}
