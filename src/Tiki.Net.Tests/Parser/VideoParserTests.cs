using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;
using Tiki.Parsers.Media;

namespace Tiki.Tests.Parser;

[TestFixture]
public class VideoParserTests
{
    private VideoParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new VideoParser();
    }

    [Test]
    public void SupportedTypes_ContainsVideoFormats()
    {
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.VideoMp4));
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.VideoAvi));
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.VideoMkv));
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.VideoMov));
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.VideoWebm));
    }

    [Test]
    public async Task ParseAsync_SampleMp4_ReturnsVideo()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.mp4");
        using var stream = File.OpenRead(filePath);
        var context = new ParseContext { FileName = "sample.mp4" };

        var result = await _parser.ParseAsync(stream, context);

        Assert.That(result, Is.InstanceOf<TikiVideo>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.VideoMp4));
    }
}
