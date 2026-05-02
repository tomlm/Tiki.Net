using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser.Parsers;

namespace Tiki.Tests.Parser;

[TestFixture]
public class XmlParserTests
{
    private XmlParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new XmlParser();
    }

    [Test]
    public void SupportedTypes_ContainsXmlTypes()
    {
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.TextXml));
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.ApplicationXml));
    }

    [Test]
    public async Task ParseAsync_SampleXml_ExtractsTextContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.xml");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiFile>());
        Assert.That(result.Content, Does.Contain("Test Book One"));
        Assert.That(result.Content, Does.Contain("Author A"));
        Assert.That(result.Content, Does.Contain("29.99"));
    }

    [Test]
    public async Task ParseAsync_WithMaxLength_Truncates()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.xml");
        using var stream = File.OpenRead(filePath);
        var context = new global::Tiki.Parser.ParseContext { MaxContentLength = 20 };

        var result = await _parser.ParseAsync(stream, context);

        Assert.That(result.Content.Length, Is.LessThanOrEqualTo(20));
    }
}
