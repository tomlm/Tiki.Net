namespace Tiki.Content;

/// <summary>
/// Receives notification of the logical content of a parsed document.
/// Analogous to SAX ContentHandler in the Java Tiki implementation.
/// </summary>
public interface IContentHandler
{
    void StartDocument();
    void EndDocument();
    void StartElement(string uri, string localName, string qName, IReadOnlyDictionary<string, string>? attributes);
    void EndElement(string uri, string localName, string qName);
    void Characters(ReadOnlySpan<char> characters);
    void IgnorableWhitespace(ReadOnlySpan<char> whitespace);
}
