namespace Tiki.Parsers.Image;

/// <summary>
/// Extension methods for adding image metadata parser to TikiConfig.
/// </summary>
public static class ImageTikiExtensions
{
    public static TikiConfig.Builder AddImageParser(this TikiConfig.Builder builder)
    {
        builder.AddParser(new ImageMetadataParser());
        return builder;
    }
}
