using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parsers.Html;

namespace Tiki.Tests.Parser;

[TestFixture]
public class HtmlParserTests
{
    private HtmlParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new HtmlParser();
    }

    [Test]
    public void SupportedTypes_ContainsTextHtml()
    {
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.TextHtml));
    }

    [Test]
    public async Task ParseAsync_SampleHtml_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.html");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiWebPage>());
        Assert.That(result.Content, Does.Contain("Welcome to the Sample Page"));
        Assert.That(result.Content, Does.Contain("paragraph of text content"));
    }

    [Test]
    public async Task ParseAsync_ExtractsMetadata()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.html");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);
        var webPage = result as TikiWebPage;

        Assert.That(webPage, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Sample HTML Page"));
        Assert.That(result.Authors, Does.Contain("Test Author"));
        Assert.That(result.Description, Is.EqualTo("A sample HTML page for testing"));
        Assert.That(result.Keywords, Does.Contain("test"));
        Assert.That(webPage!.Language, Is.EqualTo("en"));
        Assert.That(webPage.Generator, Is.EqualTo("Tiki.Net Tests"));
        Assert.That(webPage.Charset, Is.EqualTo("utf-8"));
    }

    [Test]
    public async Task ParseAsync_ExtractsLinks()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.html");
        using var stream = File.OpenRead(filePath);

        var result = (TikiWebPage)await _parser.ParseAsync(stream);

        Assert.That(result.Links, Is.Not.Null);
        Assert.That(result.Links, Does.Contain("https://example.com"));
    }

    [Test]
    public async Task ParseAsync_ExcludesScriptAndStyle()
    {
        var html = "<html><body><script>var x = 1;</script><style>.a{}</style><p>Visible text</p></body></html>";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));

        var result = await _parser.ParseAsync(stream);

        Assert.That(result.Content, Does.Contain("Visible text"));
        Assert.That(result.Content, Does.Not.Contain("var x"));
        Assert.That(result.Content, Does.Not.Contain(".a{}"));
    }
}
