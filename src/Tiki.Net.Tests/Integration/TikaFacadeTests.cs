using Tiki.Documents;
using Tiki.Mime;

namespace Tiki.Tests.Integration;

[TestFixture]
public class TikiFacadeTests
{
    private global::Tiki.TikiEngine _tika = null!;

    [SetUp]
    public void SetUp()
    {
        _tika = new global::Tiki.TikiEngine();
    }

    [Test]
    public async Task ParseAsync_TextFile_ReturnsDocument()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.txt");

        var result = await _tika.ParseAsync(filePath);

        Assert.That(result, Is.InstanceOf<TikiFile>());
        Assert.That(result.Content, Does.Contain("sample text file"));
    }

    [Test]
    public async Task ParseAsync_HtmlFile_ReturnsWebPage()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.html");

        var result = await _tika.ParseAsync(filePath);

        Assert.That(result, Is.InstanceOf<TikiWebPage>());
        Assert.That(result.Title, Is.EqualTo("Sample HTML Page"));
        Assert.That(result.Content, Does.Contain("Welcome to the Sample Page"));
    }

    [Test]
    public async Task ParseAsync_XmlFile_ReturnsDocument()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.xml");

        var result = await _tika.ParseAsync(filePath);

        Assert.That(result, Is.InstanceOf<TikiFile>());
        Assert.That(result.Content, Does.Contain("Test Book One"));
    }

    [Test]
    public async Task ParseToStringAsync_ReturnsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.txt");

        var content = await _tika.ParseToStringAsync(filePath);

        Assert.That(content, Does.Contain("sample text file"));
    }

    [Test]
    public async Task DetectAsync_TextFile_ReturnsTextPlain()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.txt");

        var mediaType = await _tika.DetectAsync(filePath);

        Assert.That(mediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task DetectAsync_HtmlFile_ReturnsTextHtml()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.html");

        var mediaType = await _tika.DetectAsync(filePath);

        Assert.That(mediaType, Is.EqualTo(MediaType.TextHtml));
    }

    [Test]
    public async Task DetectAsync_XmlFile_ReturnsTextXml()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.xml");

        var mediaType = await _tika.DetectAsync(filePath);

        Assert.That(mediaType, Is.EqualTo(MediaType.TextXml));
    }

    [Test]
    public async Task DetectAsync_PdfMagicBytes_ReturnsPdf()
    {
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };
        using var stream = new MemoryStream(pdfHeader);

        var mediaType = await _tika.DetectAsync(stream);

        Assert.That(mediaType, Is.EqualTo(MediaType.ApplicationPdf));
    }

    [Test]
    public async Task ParseAsync_WithMaxStringLength_TruncatesContent()
    {
        _tika.MaxStringLength = 15;
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.txt");

        var result = await _tika.ParseAsync(filePath);

        Assert.That(result.Content.Length, Is.LessThanOrEqualTo(15));
    }

    [Test]
    public async Task ParseAsync_Stream_Works()
    {
        var text = "Stream content for testing";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));

        var result = await _tika.ParseAsync(stream);

        Assert.That(result.Content, Does.Contain("Stream content"));
    }
}
