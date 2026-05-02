using Tiki.Detect;
using Tiki.Mime;

namespace Tiki.Tests.Detect;

[TestFixture]
public class DetectorTests
{
    private MagicBytesDetector _magicDetector = null!;
    private ExtensionDetector _extensionDetector = null!;

    [SetUp]
    public void SetUp()
    {
        _magicDetector = new MagicBytesDetector();
        _extensionDetector = new ExtensionDetector();
    }

    [Test]
    public async Task MagicBytes_Pdf_DetectsCorrectly()
    {
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 }; // %PDF-1.4
        using var stream = new MemoryStream(pdfHeader);

        var result = await _magicDetector.DetectAsync(stream);

        Assert.That(result, Is.EqualTo(MediaType.ApplicationPdf));
    }

    [Test]
    public async Task MagicBytes_Png_DetectsCorrectly()
    {
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        using var stream = new MemoryStream(pngHeader);

        var result = await _magicDetector.DetectAsync(stream);

        Assert.That(result, Is.EqualTo(MediaType.ImagePng));
    }

    [Test]
    public async Task MagicBytes_Jpeg_DetectsCorrectly()
    {
        var jpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        using var stream = new MemoryStream(jpegHeader);

        var result = await _magicDetector.DetectAsync(stream);

        Assert.That(result, Is.EqualTo(MediaType.ImageJpeg));
    }

    [Test]
    public async Task MagicBytes_Zip_DetectsCorrectly()
    {
        var zipHeader = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00 };
        using var stream = new MemoryStream(zipHeader);

        var result = await _magicDetector.DetectAsync(stream);

        Assert.That(result, Is.EqualTo(MediaType.ApplicationZip));
    }

    [Test]
    public async Task MagicBytes_Rtf_DetectsCorrectly()
    {
        var rtfHeader = new byte[] { 0x7B, 0x5C, 0x72, 0x74, 0x66, 0x31 }; // {\rtf1
        using var stream = new MemoryStream(rtfHeader);

        var result = await _magicDetector.DetectAsync(stream);

        Assert.That(result, Is.EqualTo(MediaType.ApplicationRtf));
    }

    [Test]
    public async Task MagicBytes_PlainText_DetectsAsText()
    {
        var text = "Hello, this is plain text content."u8.ToArray();
        using var stream = new MemoryStream(text);

        var result = await _magicDetector.DetectAsync(stream);

        Assert.That(result, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task MagicBytes_NullStream_ReturnsOctetStream()
    {
        var result = await _magicDetector.DetectAsync(null);

        Assert.That(result, Is.EqualTo(MediaType.OctetStream));
    }

    [Test]
    public async Task Extension_Pdf_DetectsCorrectly()
    {
        var result = await _extensionDetector.DetectAsync(null, "document.pdf");
        Assert.That(result, Is.EqualTo(MediaType.ApplicationPdf));
    }

    [Test]
    public async Task Extension_Docx_DetectsCorrectly()
    {
        var result = await _extensionDetector.DetectAsync(null, "report.docx");
        Assert.That(result, Is.EqualTo(MediaType.Docx));
    }

    [Test]
    public async Task Extension_Html_DetectsCorrectly()
    {
        var result = await _extensionDetector.DetectAsync(null, "page.html");
        Assert.That(result, Is.EqualTo(MediaType.TextHtml));
    }

    [Test]
    public async Task Extension_NoExtension_ReturnsOctetStream()
    {
        var result = await _extensionDetector.DetectAsync(null, "noextension");
        Assert.That(result, Is.EqualTo(MediaType.OctetStream));
    }

    [Test]
    public async Task Extension_CaseInsensitive()
    {
        var result = await _extensionDetector.DetectAsync(null, "file.PDF");
        Assert.That(result, Is.EqualTo(MediaType.ApplicationPdf));
    }

    [Test]
    public async Task CompositeDetector_MagicOverExtension()
    {
        var composite = new CompositeDetector(new MagicBytesDetector(), new ExtensionDetector());

        // PDF magic bytes but .txt extension - magic bytes should win
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };
        using var stream = new MemoryStream(pdfHeader);

        var result = await composite.DetectAsync(stream, "fake.txt");

        Assert.That(result, Is.EqualTo(MediaType.ApplicationPdf));
    }

    [Test]
    public async Task CompositeDetector_FallsBackToExtension()
    {
        var composite = new CompositeDetector(new MagicBytesDetector(), new ExtensionDetector());

        // Unknown binary data but with .pdf extension
        var unknownData = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        using var stream = new MemoryStream(unknownData);

        var result = await composite.DetectAsync(stream, "document.pdf");

        Assert.That(result, Is.EqualTo(MediaType.ApplicationPdf));
    }
}
