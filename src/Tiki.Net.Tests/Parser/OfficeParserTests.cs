using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;
using Tiki.Parsers.Office;

namespace Tiki.Tests.Parser;

[TestFixture]
public class OfficeParserTests
{
    private OoxmlParser _ooxmlParser = null!;

    [SetUp]
    public void SetUp()
    {
        _ooxmlParser = new OoxmlParser();
    }

    [Test]
    public async Task ParseAsync_Docx_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.docx");
        using var stream = File.OpenRead(filePath);
        var context = new ParseContext { FileName = "sample.docx" };

        var result = await _ooxmlParser.ParseAsync(stream, context);

        Assert.That(result, Is.InstanceOf<TikiDocument>());
        Assert.That(result.Content, Does.Contain("sample Word document"));
        Assert.That(result.MediaType, Is.EqualTo(MediaType.Docx));
    }

    [Test]
    public async Task ParseAsync_Docx_ExtractsMetadata()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.docx");
        using var stream = File.OpenRead(filePath);
        var context = new ParseContext { FileName = "sample.docx" };

        var result = await _ooxmlParser.ParseAsync(stream, context);
        var doc = result as TikiDocument;

        Assert.That(doc, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Sample DOCX"));
        Assert.That(result.Authors, Does.Contain("Test Author"));
        Assert.That(doc!.Company, Is.EqualTo("Test Corp"));
    }

    [Test]
    public async Task ParseAsync_Xlsx_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.xlsx");
        using var stream = File.OpenRead(filePath);
        var context = new ParseContext { FileName = "sample.xlsx" };

        var result = await _ooxmlParser.ParseAsync(stream, context);

        Assert.That(result, Is.InstanceOf<TikiSpreadsheet>());
        Assert.That(result.Content, Does.Contain("Name"));
        Assert.That(result.MediaType, Is.EqualTo(MediaType.Xlsx));
    }

    [Test]
    public async Task ParseAsync_Xlsx_ExtractsMetadata()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.xlsx");
        using var stream = File.OpenRead(filePath);
        var context = new ParseContext { FileName = "sample.xlsx" };

        var result = await _ooxmlParser.ParseAsync(stream, context);

        Assert.That(result.Title, Is.EqualTo("Sample XLSX"));
        Assert.That(result.Authors, Does.Contain("Test Author"));
    }

    [Test]
    public async Task ParseAsync_Pptx_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.pptx");
        using var stream = File.OpenRead(filePath);
        var context = new ParseContext { FileName = "sample.pptx" };

        var result = await _ooxmlParser.ParseAsync(stream, context);

        Assert.That(result, Is.InstanceOf<TikiPresentation>());
        Assert.That(result.Content, Does.Contain("Sample Presentation"));
        Assert.That(result.MediaType, Is.EqualTo(MediaType.Pptx));
    }

    [Test]
    public async Task ParseAsync_Pptx_ExtractsMetadata()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.pptx");
        using var stream = File.OpenRead(filePath);
        var context = new ParseContext { FileName = "sample.pptx" };

        var result = await _ooxmlParser.ParseAsync(stream, context);
        var pres = result as TikiPresentation;

        Assert.That(result.Title, Is.EqualTo("Sample PPTX"));
        Assert.That(result.Authors, Does.Contain("Test Author"));
        Assert.That(pres!.SlideCount, Is.EqualTo(1));
    }
}
