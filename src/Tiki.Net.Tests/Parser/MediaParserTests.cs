using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;
using Tiki.Parsers.Media;

namespace Tiki.Tests.Parser;

[TestFixture]
public class MediaParserTests
{
    private AudioParser _audioParser = null!;

    [SetUp]
    public void SetUp()
    {
        _audioParser = new AudioParser();
    }

    [Test]
    public void SupportedTypes_ContainsAudioFormats()
    {
        Assert.That(_audioParser.SupportedTypes, Does.Contain(MediaType.AudioMpeg));
        Assert.That(_audioParser.SupportedTypes, Does.Contain(MediaType.AudioFlac));
        Assert.That(_audioParser.SupportedTypes, Does.Contain(MediaType.AudioWav));
    }

    [Test]
    public async Task ParseAsync_SampleMp3_ReturnsMusic()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.mp3");
        using var stream = File.OpenRead(filePath);
        var context = new ParseContext { FileName = "sample.mp3" };

        var result = await _audioParser.ParseAsync(stream, context);

        Assert.That(result, Is.InstanceOf<TikiMusic>());
    }

    [Test]
    public async Task ParseAsync_SampleMp3_ExtractsId3Tags()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.mp3");
        using var stream = File.OpenRead(filePath);
        var context = new ParseContext { FileName = "sample.mp3" };

        var result = await _audioParser.ParseAsync(stream, context);
        var music = result as TikiMusic;

        Assert.That(music, Is.Not.Null);
        Assert.That(music!.Title, Is.EqualTo("Sample Song"));
        Assert.That(music.Artist, Is.EqualTo("Test Artist"));
        Assert.That(music.Album, Is.EqualTo("Test Album"));
        Assert.That(music.TrackNumber, Is.EqualTo(5));
        Assert.That(music.Year, Is.EqualTo(2023));
        Assert.That(music.Genre, Does.Contain("Rock"));
    }
}
