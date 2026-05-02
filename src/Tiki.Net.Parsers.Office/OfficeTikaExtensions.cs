namespace Tiki.Parsers.Office;

/// <summary>
/// Extension methods for adding Office parsers to TikiConfig.
/// </summary>
public static class OfficeTikiExtensions
{
    /// <summary>
    /// Adds parsers for OOXML (DOCX, XLSX, PPTX) and OpenDocument (ODT, ODS, ODP) formats.
    /// </summary>
    public static TikiConfig.Builder AddOfficeParser(this TikiConfig.Builder builder)
    {
        builder.AddParser(new OoxmlParser());
        builder.AddParser(new OdtParser());
        return builder;
    }
}
