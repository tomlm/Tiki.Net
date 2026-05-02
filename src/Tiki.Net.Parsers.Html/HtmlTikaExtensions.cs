namespace Tiki.Parsers.Html;

/// <summary>
/// Extension methods for adding HTML parser to TikiConfig.
/// </summary>
public static class HtmlTikiExtensions
{
    public static TikiConfig.Builder AddHtmlParser(this TikiConfig.Builder builder)
    {
        builder.AddParser(new HtmlParser());
        return builder;
    }
}
