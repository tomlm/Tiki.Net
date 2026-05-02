using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parsers.Image;

namespace Tiki.Tests.Parser;

[TestFixture]
public class ImageParserTests
{
    private ImageMetadataParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new ImageMetadataParser();
    }

    [Test]
    public void SupportedTypes_ContainsImageFormats()
    {
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.ImageJpeg));
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.ImagePng));
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.ImageTiff));
    }

    [Test]
    public async Task ParseAsync_SampleJpeg_ReturnsPhoto()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.jpg");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiPhoto>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.ImageJpeg));
    }

    [Test]
    public async Task ParseAsync_SampleJpeg_ExtractsExifMetadata()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.jpg");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);
        var photo = result as TikiPhoto;

        Assert.That(photo, Is.Not.Null);
        Assert.That(photo!.CameraManufacturer, Is.EqualTo("TestCamera"));
        Assert.That(photo.CameraModel, Is.EqualTo("TC-100"));
        Assert.That(photo.Orientation, Is.EqualTo(1));
    }
}
