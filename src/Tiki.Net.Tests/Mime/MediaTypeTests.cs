using Tiki.Mime;

namespace Tiki.Tests.Mime;

[TestFixture]
public class MediaTypeTests
{
    [Test]
    public void Parse_SimpleType_ReturnsCorrectTypeAndSubtype()
    {
        var mediaType = MediaType.Parse("application/pdf");

        Assert.That(mediaType.Type, Is.EqualTo("application"));
        Assert.That(mediaType.Subtype, Is.EqualTo("pdf"));
        Assert.That(mediaType.Parameters, Is.Null);
    }

    [Test]
    public void Parse_WithParameters_ReturnsParameters()
    {
        var mediaType = MediaType.Parse("text/plain; charset=utf-8");

        Assert.That(mediaType.Type, Is.EqualTo("text"));
        Assert.That(mediaType.Subtype, Is.EqualTo("plain"));
        Assert.That(mediaType.Parameters, Is.Not.Null);
        Assert.That(mediaType.Parameters!["charset"], Is.EqualTo("utf-8"));
    }

    [Test]
    public void Parse_CaseInsensitive_NormalizesToLower()
    {
        var mediaType = MediaType.Parse("Application/PDF");

        Assert.That(mediaType.Type, Is.EqualTo("application"));
        Assert.That(mediaType.Subtype, Is.EqualTo("pdf"));
    }

    [Test]
    public void Parse_InvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MediaType.Parse("invalid"));
    }

    [Test]
    public void Equals_SameTypesDifferentCase_AreEqual()
    {
        var a = new MediaType("Application", "PDF");
        var b = new MediaType("application", "pdf");

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_DifferentTypes_AreNotEqual()
    {
        Assert.That(MediaType.TextPlain, Is.Not.EqualTo(MediaType.ApplicationPdf));
    }

    [Test]
    public void ToString_SimpleType_ReturnsCorrectFormat()
    {
        Assert.That(MediaType.ApplicationPdf.ToString(), Is.EqualTo("application/pdf"));
    }

    [Test]
    public void ToString_WithParameters_IncludesParameters()
    {
        var mediaType = MediaType.Parse("text/plain; charset=utf-8");
        Assert.That(mediaType.ToString(), Does.Contain("charset=utf-8"));
    }

    [Test]
    public void StaticConstants_HaveCorrectValues()
    {
        Assert.That(MediaType.TextPlain.ToString(), Is.EqualTo("text/plain"));
        Assert.That(MediaType.TextHtml.ToString(), Is.EqualTo("text/html"));
        Assert.That(MediaType.ApplicationPdf.ToString(), Is.EqualTo("application/pdf"));
        Assert.That(MediaType.ImageJpeg.ToString(), Is.EqualTo("image/jpeg"));
        Assert.That(MediaType.AudioMpeg.ToString(), Is.EqualTo("audio/mpeg"));
        Assert.That(MediaType.VideoMp4.ToString(), Is.EqualTo("video/mp4"));
    }
}
