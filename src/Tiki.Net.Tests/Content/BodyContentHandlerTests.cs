using Tiki.Content;

namespace Tiki.Tests.Content;

[TestFixture]
public class BodyContentHandlerTests
{
    [Test]
    public void Characters_InBody_ExtractsText()
    {
        var handler = new BodyContentHandler();

        handler.StartDocument();
        handler.StartElement("", "html", "html", null);
        handler.StartElement("", "body", "body", null);
        handler.Characters("Hello, World!".AsSpan());
        handler.EndElement("", "body", "body");
        handler.EndElement("", "html", "html");
        handler.EndDocument();

        Assert.That(handler.ToString(), Is.EqualTo("Hello, World!"));
    }

    [Test]
    public void Characters_OutsideBody_Ignored()
    {
        var handler = new BodyContentHandler();

        handler.StartDocument();
        handler.StartElement("", "html", "html", null);
        handler.StartElement("", "head", "head", null);
        handler.Characters("Title text".AsSpan());
        handler.EndElement("", "head", "head");
        handler.StartElement("", "body", "body", null);
        handler.Characters("Body text".AsSpan());
        handler.EndElement("", "body", "body");
        handler.EndElement("", "html", "html");
        handler.EndDocument();

        Assert.That(handler.ToString(), Is.EqualTo("Body text"));
    }

    [Test]
    public void BlockElements_AddNewlines()
    {
        var handler = new BodyContentHandler();

        handler.StartDocument();
        handler.StartElement("", "body", "body", null);
        handler.StartElement("", "p", "p", null);
        handler.Characters("First paragraph".AsSpan());
        handler.EndElement("", "p", "p");
        handler.StartElement("", "p", "p", null);
        handler.Characters("Second paragraph".AsSpan());
        handler.EndElement("", "p", "p");
        handler.EndElement("", "body", "body");
        handler.EndDocument();

        var text = handler.ToString();
        Assert.That(text, Does.Contain("First paragraph"));
        Assert.That(text, Does.Contain("Second paragraph"));
        Assert.That(text, Does.Contain("\n"));
    }

    [Test]
    public void MaxLength_TruncatesContent()
    {
        var handler = new BodyContentHandler(maxLength: 5);

        handler.StartDocument();
        handler.StartElement("", "body", "body", null);
        handler.Characters("Hello, World! This is too long.".AsSpan());
        handler.EndElement("", "body", "body");
        handler.EndDocument();

        Assert.That(handler.ToString().Length, Is.LessThanOrEqualTo(5));
    }
}
