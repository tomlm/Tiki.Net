using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parsers.Email;

namespace Tiki.Tests.Parser;

[TestFixture]
public class EmailParserTests
{
    private EmailParser _parser = null!;

    [SetUp]
    public void SetUp()
    {
        _parser = new EmailParser();
    }

    [Test]
    public void SupportedTypes_ContainsRfc822()
    {
        Assert.That(_parser.SupportedTypes, Does.Contain(MediaType.MessageRfc822));
    }

    [Test]
    public async Task ParseAsync_SampleEml_ExtractsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.eml");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);

        Assert.That(result, Is.InstanceOf<TikiMessage>());
        Assert.That(result.Content, Does.Contain("sample email"));
    }

    [Test]
    public async Task ParseAsync_SampleEml_ExtractsMetadata()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.eml");
        using var stream = File.OpenRead(filePath);

        var result = await _parser.ParseAsync(stream);
        var msg = result as TikiMessage;

        Assert.That(msg, Is.Not.Null);
        Assert.That(msg!.FromAddress, Is.EqualTo("sender@example.com"));
        Assert.That(msg.ToAddresses, Does.Contain("recipient@example.com"));
        Assert.That(msg.Subject, Is.EqualTo("Sample Email"));
    }
}
