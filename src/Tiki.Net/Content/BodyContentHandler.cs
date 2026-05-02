using System.Text;

namespace Tiki.Content;

/// <summary>
/// A content handler that extracts the body text content from a parsed document.
/// </summary>
public sealed class BodyContentHandler : IContentHandler
{
    private readonly StringBuilder _builder;
    private readonly int _maxLength;
    private bool _inBody;
    private int _elementDepth;

    public BodyContentHandler(int maxLength = int.MaxValue)
    {
        _builder = new StringBuilder();
        _maxLength = maxLength;
    }

    public void StartDocument()
    {
        _builder.Clear();
        _inBody = false;
        _elementDepth = 0;
    }

    public void EndDocument() { }

    public void StartElement(string uri, string localName, string qName, IReadOnlyDictionary<string, string>? attributes)
    {
        if (string.Equals(localName, "body", StringComparison.OrdinalIgnoreCase))
        {
            _inBody = true;
        }

        if (_inBody)
        {
            _elementDepth++;
            // Add whitespace between block elements
            if (IsBlockElement(localName) && _builder.Length > 0 && _builder[^1] != '\n')
            {
                AppendIfAllowed('\n');
            }
        }
    }

    public void EndElement(string uri, string localName, string qName)
    {
        if (_inBody)
        {
            _elementDepth--;
            if (IsBlockElement(localName) && _builder.Length > 0 && _builder[^1] != '\n')
            {
                AppendIfAllowed('\n');
            }

            if (string.Equals(localName, "body", StringComparison.OrdinalIgnoreCase))
            {
                _inBody = false;
            }
        }
    }

    public void Characters(ReadOnlySpan<char> characters)
    {
        if (_inBody && _builder.Length < _maxLength)
        {
            var remaining = _maxLength - _builder.Length;
            var toAppend = characters.Length <= remaining ? characters : characters[..remaining];
            _builder.Append(toAppend);
        }
    }

    public void IgnorableWhitespace(ReadOnlySpan<char> whitespace)
    {
        if (_inBody)
        {
            Characters(whitespace);
        }
    }

    public override string ToString() => _builder.ToString();

    private void AppendIfAllowed(char c)
    {
        if (_builder.Length < _maxLength)
            _builder.Append(c);
    }

    private static bool IsBlockElement(string localName) =>
        localName is "p" or "div" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6"
            or "ul" or "ol" or "li" or "table" or "tr" or "br" or "hr"
            or "blockquote" or "pre" or "section" or "article";
}
