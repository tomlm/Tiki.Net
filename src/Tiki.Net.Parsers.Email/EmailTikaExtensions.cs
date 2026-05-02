namespace Tiki.Parsers.Email;

/// <summary>
/// Extension methods for adding email parser to TikiConfig.
/// </summary>
public static class EmailTikiExtensions
{
    public static TikiConfig.Builder AddEmailParser(this TikiConfig.Builder builder)
    {
        builder.AddParser(new EmailParser());
        return builder;
    }
}
