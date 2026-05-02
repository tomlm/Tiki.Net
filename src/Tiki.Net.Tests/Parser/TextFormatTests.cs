using Tiki.Mime;
using Tiki.Parser;
using Tiki.Parser.Parsers;

namespace Tiki.Tests.Parser;

[TestFixture]
public class TextFormatTests
{
    private TextParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new TextParser();
    }

    [Test]
    public async Task ParseAsync_CSharpFile_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.cs");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiFile>());
        Assert.That(result.Content, Does.Contain("SampleClass"));
        Assert.That(result.Content, Does.Contain("Calculate"));
        Assert.That(result.Content, Does.Contain("namespace SampleNamespace"));
    }

    [Test]
    public async Task ParseAsync_CppFile_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.cpp");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiFile>());
        Assert.That(result.Content, Does.Contain("SampleClass"));
        Assert.That(result.Content, Does.Contain("#include"));
        Assert.That(result.Content, Does.Contain("int main"));
    }

    [Test]
    public async Task ParseAsync_MarkdownFile_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.md");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiFile>());
        Assert.That(result.Content, Does.Contain("Sample Markdown Document"));
        Assert.That(result.Content, Does.Contain("sample markdown"));
        Assert.That(result.Content, Does.Contain("Item one"));
    }

    [Test]
    public async Task ParseAsync_JsonFile_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.json");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiFile>());
        Assert.That(result.Content, Does.Contain("Sample JSON"));
        Assert.That(result.Content, Does.Contain("alpha"));
        Assert.That(result.Content, Does.Contain("beta"));
    }

    [Test]
    public async Task DetectAsync_CSharpFile_ReturnsTextPlain()
    {
        var tika = new global::Tiki.TikiEngine();
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.cs");

        var mediaType = await tika.DetectAsync(filePath);

        Assert.That(mediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task DetectAsync_MarkdownFile_ReturnsTextPlain()
    {
        var tika = new global::Tiki.TikiEngine();
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.md");

        var mediaType = await tika.DetectAsync(filePath);

        Assert.That(mediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task DetectAsync_JsonFile_ReturnsApplicationJson()
    {
        var tika = new global::Tiki.TikiEngine();
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.json");

        var mediaType = await tika.DetectAsync(filePath);

        Assert.That(mediaType, Is.EqualTo(MediaType.ApplicationJson));
    }
}
