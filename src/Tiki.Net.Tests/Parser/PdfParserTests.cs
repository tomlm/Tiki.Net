using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parsers.Pdf;

namespace Tiki.Tests.Parser;

[TestFixture]
public class PdfParserTests
{
    private PdfParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new PdfParser();
    }

    [Test]
    public void SupportedTypes_ContainsPdf()
    {
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.ApplicationPdf));
    }

    [Test]
    public async Task ParseAsync_SamplePdf_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.pdf");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiDocument>());
        Assert.That(result.Content, Does.Contain("sample PDF document"));
        Assert.That(result.MediaType, Is.EqualTo(MediaType.ApplicationPdf));
    }

    [Test]
    public async Task ParseAsync_SamplePdf_ExtractsMetadata()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.pdf");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);
        var doc = result as TikiDocument;

        Assert.That(doc, Is.Not.Null);
        Assert.That(doc!.PageCount, Is.EqualTo(1));
    }
}
