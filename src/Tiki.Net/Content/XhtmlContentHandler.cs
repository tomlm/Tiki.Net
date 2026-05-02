using System.Text;

namespace Tiki.Content;

/// <summary>
/// A content handler that produces XHTML output from parse events.
/// </summary>
public sealed class XhtmlContentHandler : IContentHandler
{
    private readonly StringBuilder _builder;
    private readonly int _maxLength;

    public XhtmlContentHandler(int maxLength = int.MaxValue)
    {
        _builder = new StringBuilder();
        _maxLength = maxLength;
    }

    public void StartDocument()
    {
        _builder.Clear();
    }

    public void EndDocument() { }

    public void StartElement(string uri, string localName, string qName, IReadOnlyDictionary<string, string>? attributes)
    {
        if (_builder.Length >= _maxLength) return;

        _builder.Append('<').Append(localName);
        if (attributes != null)
        {
            foreach (var (key, value) in attributes)
            {
                _builder.Append(' ').Append(key).Append("=\"");
                AppendEscaped(value);
                _builder.Append('"');
            }
        }
        _builder.Append('>');
    }

    public void EndElement(string uri, string localName, string qName)
    {
        if (_builder.Length >= _maxLength) return;
        _builder.Append("</").Append(localName).Append('>');
    }

    public void Characters(ReadOnlySpan<char> characters)
    {
        if (_builder.Length >= _maxLength) return;

        foreach (var c in characters)
        {
            switch (c)
            {
                case '<': _builder.Append("&lt;"); break;
                case '>': _builder.Append("&gt;"); break;
                case '&': _builder.Append("&amp;"); break;
                default: _builder.Append(c); break;
            }
        }
    }

    public void IgnorableWhitespace(ReadOnlySpan<char> whitespace)
    {
        Characters(whitespace);
    }

    public override string ToString() => _builder.ToString();

    private void AppendEscaped(string value)
    {
        foreach (var c in value)
        {
            switch (c)
            {
                case '"': _builder.Append("&quot;"); break;
                case '&': _builder.Append("&amp;"); break;
                case '<': _builder.Append("&lt;"); break;
                default: _builder.Append(c); break;
            }
        }
    }
}
