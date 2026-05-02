using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;
using Tiki.Parser.Parsers;

namespace Tiki.Tests.Parser;

[TestFixture]
public class TextParserTests
{
    private TextParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new TextParser();
    }

    [Test]
    public void SupportedTypes_ContainsTextPlain()
    {
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.TextPlain));
    }

    [Test]
    public async Task ParseAsync_SimpleText_ExtractsContent()
    {
        var text = "Hello, World!\nSecond line.";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiFile>());
        Assert.That(result.Content, Is.EqualTo(text));
        Assert.That(result.MediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task ParseAsync_WithMaxLength_TruncatesContent()
    {
        var text = "This is a longer text that should be truncated.";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
        var context = new ParseContext { MaxContentLength = 10 };

        var result = await _parser.ParseAsync(stream, context);

        Assert.That(result.Content.Length, Is.LessThanOrEqualTo(10));
    }

    [Test]
    public async Task ParseAsync_FromFile_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.txt");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result.Content, Does.Contain("sample text file"));
        Assert.That(result.Content, Does.Contain("quick brown fox"));
    }

    [Test]
    public async Task ParseAsync_EmptyStream_ReturnsEmptyContent()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _parser.ParseAsync(stream);

        Assert.That(result.Content, Is.Empty);
    }

    [Test]
    public async Task ParseAsync_CancellationToken_Respected()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var text = "Some content";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));

        Assert.ThrowsAsync<TaskCanceledException>(
            async () => await _parser.ParseAsync(stream, cancellationToken: cts.Token));
    }
}
