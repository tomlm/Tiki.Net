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

    private const string CsvText = "name,age\nAlice,30\nBob,25\n";
    private static readonly MediaType s_csv = MediaType.Parse("text/csv");

    [Test]
    public async Task ParseAsync_CsvStreamWithoutFileName_ReturnsPlainText()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var result = await _tika.ParseAsync(stream);

        Assert.That(result, Is.Not.InstanceOf<TikiData>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task ParseAsync_CsvStreamWithFileName_ReturnsCsvData()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var result = await _tika.ParseAsync(stream, "people.csv");

        Assert.That(result, Is.InstanceOf<TikiData>());
        Assert.That(result.MediaType, Is.EqualTo(s_csv));
        Assert.That(result.Content, Does.Contain("Alice,30"));
    }

    [Test]
    public async Task ParseAsync_StreamWithNullFileName_BehavesLikeStreamOnly()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var result = await _tika.ParseAsync(stream, null);

        Assert.That(result.MediaType, Is.EqualTo(MediaType.TextPlain));
        Assert.That(result.Content, Does.Contain("Alice,30"));
    }

    [Test]
    public async Task ParseAsync_StreamWithDefaultLiteral_StillBindsToCancellationTokenOverload()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        // Existing call shape; must keep compiling (not ambiguous with the fileName overload).
        var result = await _tika.ParseAsync(stream, default);

        Assert.That(result.MediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task ParseAsync_DocxStreamWithFileName_ReturnsDocument()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.docx");
        using var stream = new MemoryStream(await File.ReadAllBytesAsync(filePath));

        var result = await _tika.ParseAsync(stream, "attachment.docx");

        Assert.That(result, Is.InstanceOf<TikiDocument>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.Docx));
    }

    [Test]
    public async Task ParseAsync_NonSeekableStreamWithFileName_ReturnsCsvData()
    {
        using var inner = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));
        using var stream = new System.IO.Compression.DeflateStream(Compress(inner.ToArray()), System.IO.Compression.CompressionMode.Decompress);

        var result = await _tika.ParseAsync(stream, "people.csv");

        Assert.That(result, Is.InstanceOf<TikiData>());
        Assert.That(result.MediaType, Is.EqualTo(s_csv));
    }

    [Test]
    public async Task ParseToStringAsync_StreamWithFileName_ReturnsContent()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var content = await _tika.ParseToStringAsync(stream, "people.csv");

        Assert.That(content, Does.Contain("Bob,25"));
    }

    // A name that disagrees with the content. Bytes that identify a format win; the name decides only
    // what the bytes cannot (which text format, which format inside a ZIP).

    [Test]
    public async Task ParseAsync_PdfStreamNamedTxt_ReturnsPdfDocument()
    {
        using var stream = await OpenTestDataAsync("sample.pdf");

        var result = await _tika.ParseAsync(stream, "notes.txt");

        Assert.That(result, Is.InstanceOf<TikiDocument>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.ApplicationPdf));
        Assert.That(result.Content, Does.Not.Contain("%PDF"));
    }

    [Test]
    public async Task DetectAsync_PdfStreamNamedTxt_ReturnsPdf()
    {
        using var stream = await OpenTestDataAsync("sample.pdf");

        var mediaType = await _tika.DetectAsync(stream, "notes.txt");

        Assert.That(mediaType, Is.EqualTo(MediaType.ApplicationPdf));
    }

    [Test]
    public async Task ParseAsync_JpegStreamNamedTxt_ReturnsPhoto()
    {
        using var stream = await OpenTestDataAsync("sample.jpg");

        var result = await _tika.ParseAsync(stream, "image.txt");

        Assert.That(result, Is.InstanceOf<TikiPhoto>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.ImageJpeg));
    }

    [Test]
    public async Task ParseAsync_HtmlStreamNamedTxt_ReturnsWebPage()
    {
        using var stream = await OpenTestDataAsync("sample.html");

        var result = await _tika.ParseAsync(stream, "page.txt");

        Assert.That(result, Is.InstanceOf<TikiWebPage>());
        Assert.That(result.Title, Is.EqualTo("Sample HTML Page"));
    }

    [Test]
    public async Task ParseAsync_TextStreamNamedPdf_ReturnsText()
    {
        using var stream = await OpenTestDataAsync("sample.txt");

        var result = await _tika.ParseAsync(stream, "x.pdf");

        Assert.That(result.MediaType, Is.EqualTo(MediaType.TextPlain));
        Assert.That(result.Content, Does.Contain("sample text file"));
    }

    [Test]
    public async Task DetectAsync_TextMentioningHtmlWithFileName_NameDecides()
    {
        // "<html" in the first bytes is not enough to overrule the name; only a document that opens as HTML is.
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Wrap the page in <html> and <body> tags.\n"));

        var mediaType = await _tika.DetectAsync(stream, "notes.md");

        Assert.That(mediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task DetectAsync_CsvStartingWithBmpSignatureAndFileName_ReturnsTextCsv()
    {
        // "BM" opens BMP files too; it must not take a CSV away from its name.
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("BMI,Age\n22.5,30\n27.1,45\n"));

        var mediaType = await _tika.DetectAsync(stream, "health.csv");

        Assert.That(mediaType, Is.EqualTo(s_csv));
    }

    [Test]
    public async Task ParseAsync_JsonStreamWithFileName_ReturnsJsonData()
    {
        using var stream = await OpenTestDataAsync("sample.json");

        var result = await _tika.ParseAsync(stream, "data.json");

        Assert.That(result, Is.InstanceOf<TikiData>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.ApplicationJson));
    }

    [Test]
    public async Task DetectAsync_CsvStreamWithoutFileName_ReturnsTextPlain()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var mediaType = await _tika.DetectAsync(stream);

        Assert.That(mediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task DetectAsync_CsvStreamWithFileName_ReturnsTextCsv()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var mediaType = await _tika.DetectAsync(stream, "people.csv");

        Assert.That(mediaType, Is.EqualTo(s_csv));
    }

    [Test]
    public async Task DetectAsync_JsonStreamWithFileName_ReturnsApplicationJson()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"a\": 1}"));

        var mediaType = await _tika.DetectAsync(stream, "data.json");

        Assert.That(mediaType, Is.EqualTo(MediaType.ApplicationJson));
    }

    private static async Task<MemoryStream> OpenTestDataAsync(string name)
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", name);
        return new MemoryStream(await File.ReadAllBytesAsync(filePath));
    }

    private static MemoryStream Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var deflate = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionMode.Compress, leaveOpen: true))
            deflate.Write(data, 0, data.Length);
        output.Position = 0;
        return output;
    }
}
