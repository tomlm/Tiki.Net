using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parsers.Rtf;

namespace Tiki.Tests.Parser;

[TestFixture]
public class RtfParserTests
{
    private RtfParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new RtfParser();
    }

    [Test]
    public void SupportedTypes_ContainsRtf()
    {
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.ApplicationRtf));
    }

    [Test]
    public async Task ParseAsync_SampleRtf_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.rtf");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiDocument>());
        Assert.That(result.Content, Does.Contain("sample RTF document"));
        Assert.That(result.MediaType, Is.EqualTo(MediaType.ApplicationRtf));
    }
}
