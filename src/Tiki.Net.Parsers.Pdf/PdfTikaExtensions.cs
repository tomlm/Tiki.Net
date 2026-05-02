namespace Tiki.Parsers.Pdf;

/// <summary>
/// Extension methods for adding PDF parser to TikiConfig.
/// </summary>
public static class PdfTikiExtensions
{
    public static TikiConfig.Builder AddPdfParser(this TikiConfig.Builder builder)
    {
        builder.AddParser(new PdfParser());
        return builder;
    }
}
