using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;
using Tiki.Parsers.Media;

namespace Tiki.Tests.Integration;

[TestFixture]
public class TikiFacadeTests
{
    private global::Tiki.TikiEngine _tika = null!;

    [SetUp]
    public void SetUp()
    {
        _tika = new global::Tiki.TikiEngine();
    }

    [Test]
    public async Task ParseAsync_TextFile_ReturnsDocument()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.txt");

        var result = await _tika.ParseAsync(filePath);

        Assert.That(result, Is.InstanceOf<TikiFile>());
        Assert.That(result.Content, Does.Contain("sample text file"));
    }

    [Test]
    public async Task ParseAsync_HtmlFile_ReturnsWebPage()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.html");

        var result = await _tika.ParseAsync(filePath);

        Assert.That(result, Is.InstanceOf<TikiWebPage>());
        Assert.That(result.Title, Is.EqualTo("Sample HTML Page"));
        Assert.That(result.Content, Does.Contain("Welcome to the Sample Page"));
    }

    [Test]
    public async Task ParseAsync_XmlFile_ReturnsDocument()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.xml");

        var result = await _tika.ParseAsync(filePath);

        Assert.That(result, Is.InstanceOf<TikiFile>());
        Assert.That(result.Content, Does.Contain("Test Book One"));
    }

    [Test]
    public async Task ParseToStringAsync_ReturnsContent()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.txt");

        var content = await _tika.ParseToStringAsync(filePath);

        Assert.That(content, Does.Contain("sample text file"));
    }

    [Test]
    public async Task DetectAsync_TextFile_ReturnsTextPlain()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.txt");

        var mediaType = await _tika.DetectAsync(filePath);

        Assert.That(mediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task DetectAsync_HtmlFile_ReturnsTextHtml()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.html");

        var mediaType = await _tika.DetectAsync(filePath);

        Assert.That(mediaType, Is.EqualTo(MediaType.TextHtml));
    }

    [Test]
    public async Task DetectAsync_XmlFile_ReturnsTextXml()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.xml");

        var mediaType = await _tika.DetectAsync(filePath);

        Assert.That(mediaType, Is.EqualTo(MediaType.TextXml));
    }

    [Test]
    public async Task DetectAsync_PdfMagicBytes_ReturnsPdf()
    {
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };
        using var stream = new MemoryStream(pdfHeader);

        var mediaType = await _tika.DetectAsync(stream);

        Assert.That(mediaType, Is.EqualTo(MediaType.ApplicationPdf));
    }

    [Test]
    public async Task ParseAsync_WithMaxStringLength_TruncatesContent()
    {
        _tika.MaxStringLength = 15;
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.txt");

        var result = await _tika.ParseAsync(filePath);

        Assert.That(result.Content.Length, Is.LessThanOrEqualTo(15));
    }

    [Test]
    public async Task ParseAsync_Stream_Works()
    {
        var text = "Stream content for testing";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));

        var result = await _tika.ParseAsync(stream);

        Assert.That(result.Content, Does.Contain("Stream content"));
    }

    private const string CsvText = "name,age\nAlice,30\nBob,25\n";
    private static readonly MediaType s_csv = MediaType.Parse("text/csv");

    [Test]
    public async Task ParseAsync_CsvStreamWithoutFileName_ReturnsPlainText()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var result = await _tika.ParseAsync(stream);

        Assert.That(result, Is.Not.InstanceOf<TikiData>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task ParseAsync_CsvStreamWithFileName_ReturnsCsvData()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var result = await _tika.ParseAsync(stream, "people.csv");

        Assert.That(result, Is.InstanceOf<TikiData>());
        Assert.That(result.MediaType, Is.EqualTo(s_csv));
        Assert.That(result.Content, Does.Contain("Alice,30"));
    }

    [Test]
    public async Task ParseAsync_StreamWithNullFileName_BehavesLikeStreamOnly()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var result = await _tika.ParseAsync(stream, null);

        Assert.That(result.MediaType, Is.EqualTo(MediaType.TextPlain));
        Assert.That(result.Content, Does.Contain("Alice,30"));
    }

    [Test]
    public async Task ParseAsync_StreamWithDefaultLiteral_StillBindsToCancellationTokenOverload()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        // Existing call shape; must keep compiling (not ambiguous with the fileName overload).
        var result = await _tika.ParseAsync(stream, default);

        Assert.That(result.MediaType, Is.EqualTo(MediaType.TextPlain));

        // Both overloads give the same result for this call, so check the binding itself. (An expression
        // tree cannot omit an optional argument, so this would not compile if it bound to the fileName
        // overload, whose token is optional.)
        System.Linq.Expressions.Expression<Func<global::Tiki.TikiEngine, Stream, Task<TikiFile>>> call = (tika, s) => tika.ParseAsync(s, default);
        var parameters = ((System.Linq.Expressions.MethodCallExpression)call.Body).Method.GetParameters();
        Assert.That(parameters.Select(p => p.ParameterType), Is.EqualTo(new[] { typeof(Stream), typeof(CancellationToken) }));
    }

    [Test]
    public async Task ParseAsync_DocxStreamWithFileName_ReturnsDocument()
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.docx");
        using var stream = new MemoryStream(await File.ReadAllBytesAsync(filePath));

        var result = await _tika.ParseAsync(stream, "attachment.docx");

        Assert.That(result, Is.InstanceOf<TikiDocument>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.Docx));
    }

    [Test]
    public async Task ParseAsync_NonSeekableStreamWithFileName_ReturnsCsvData()
    {
        using var inner = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));
        using var stream = new System.IO.Compression.DeflateStream(Compress(inner.ToArray()), System.IO.Compression.CompressionMode.Decompress);

        var result = await _tika.ParseAsync(stream, "people.csv");

        Assert.That(result, Is.InstanceOf<TikiData>());
        Assert.That(result.MediaType, Is.EqualTo(s_csv));
    }

    [Test]
    public async Task ParseToStringAsync_StreamWithFileName_ReturnsContent()
    {
        // DOCX bytes are only a ZIP until the name says which format is inside; without the name this
        // throws UnsupportedFormatException, so the test fails if the overload drops the name.
        using var stream = await OpenTestDataAsync("sample.docx");

        var content = await _tika.ParseToStringAsync(stream, "attachment.docx");

        Assert.That(content, Does.Contain("sample Word document"));
    }

    // A name that disagrees with the content. Bytes that identify a format win; the name decides only
    // what the bytes cannot (which text format, which format inside a ZIP).

    [Test]
    public async Task ParseAsync_PdfStreamNamedTxt_ReturnsPdfDocument()
    {
        using var stream = await OpenTestDataAsync("sample.pdf");

        var result = await _tika.ParseAsync(stream, "notes.txt");

        Assert.That(result, Is.InstanceOf<TikiDocument>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.ApplicationPdf));
        Assert.That(result.Content, Does.Not.Contain("%PDF"));
    }

    [Test]
    public async Task DetectAsync_PdfStreamNamedTxt_ReturnsPdf()
    {
        using var stream = await OpenTestDataAsync("sample.pdf");

        var mediaType = await _tika.DetectAsync(stream, "notes.txt");

        Assert.That(mediaType, Is.EqualTo(MediaType.ApplicationPdf));
    }

    [Test]
    public async Task ParseAsync_JpegStreamNamedTxt_ReturnsPhoto()
    {
        using var stream = await OpenTestDataAsync("sample.jpg");

        var result = await _tika.ParseAsync(stream, "image.txt");

        Assert.That(result, Is.InstanceOf<TikiPhoto>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.ImageJpeg));
    }

    [Test]
    public async Task ParseAsync_HtmlStreamNamedTxt_ReturnsWebPage()
    {
        using var stream = await OpenTestDataAsync("sample.html");

        var result = await _tika.ParseAsync(stream, "page.txt");

        Assert.That(result, Is.InstanceOf<TikiWebPage>());
        Assert.That(result.Title, Is.EqualTo("Sample HTML Page"));
    }

    [Test]
    public async Task ParseAsync_TextStreamNamedPdf_ReturnsText()
    {
        using var stream = await OpenTestDataAsync("sample.txt");

        var result = await _tika.ParseAsync(stream, "x.pdf");

        Assert.That(result.MediaType, Is.EqualTo(MediaType.TextPlain));
        Assert.That(result.Content, Does.Contain("sample text file"));
    }

    [Test]
    public async Task DetectAsync_TextMentioningHtmlWithFileName_NameDecides()
    {
        // "<html" in the first bytes is not enough to overrule the name; only a document that opens as HTML is.
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Wrap the page in <html> and <body> tags.\n"));

        var mediaType = await _tika.DetectAsync(stream, "notes.md");

        Assert.That(mediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task DetectAsync_CsvStartingWithBmpSignatureAndFileName_ReturnsTextCsv()
    {
        // "BM" opens BMP files too; it must not take a CSV away from its name.
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("BMI,Age\n22.5,30\n27.1,45\n"));

        var mediaType = await _tika.DetectAsync(stream, "health.csv");

        Assert.That(mediaType, Is.EqualTo(s_csv));
    }

    [Test]
    public async Task ParseAsync_JsonStreamWithFileName_ReturnsJsonData()
    {
        using var stream = await OpenTestDataAsync("sample.json");

        var result = await _tika.ParseAsync(stream, "data.json");

        Assert.That(result, Is.InstanceOf<TikiData>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.ApplicationJson));
    }

    // A name TagLib cannot use (unknown or no extension) must not break a media parse that works without one.

    [TestCase("voicemail.dat")]
    [TestCase("voicemail")]
    [TestCase("")]
    [TestCase("voicemail.wav")]
    public async Task ParseAsync_Mp3StreamWithUnhelpfulFileName_ReturnsMusic(string fileName)
    {
        using var stream = await OpenTestDataAsync("sample.mp3");

        var result = await _tika.ParseAsync(stream, fileName);

        Assert.That(result, Is.InstanceOf<TikiMusic>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.AudioMpeg));
        Assert.That(result.Title, Is.EqualTo("Sample Song"));
    }

    [TestCase("clip.bin")]
    [TestCase("clip")]
    [TestCase("")]
    public async Task ParseAsync_Mp4StreamWithUnhelpfulFileName_ReturnsVideo(string fileName)
    {
        using var stream = await OpenTestDataAsync("sample.mp4");

        var result = await _tika.ParseAsync(stream, fileName);

        Assert.That(result, Is.InstanceOf<TikiVideo>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.VideoMp4));
    }

    [Test]
    public async Task DetectAsync_CsvStreamWithoutFileName_ReturnsTextPlain()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var mediaType = await _tika.DetectAsync(stream);

        Assert.That(mediaType, Is.EqualTo(MediaType.TextPlain));
    }

    [Test]
    public async Task DetectAsync_CsvStreamWithFileName_ReturnsTextCsv()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CsvText));

        var mediaType = await _tika.DetectAsync(stream, "people.csv");

        Assert.That(mediaType, Is.EqualTo(s_csv));
    }

    [Test]
    public async Task DetectAsync_JsonStreamWithFileName_ReturnsApplicationJson()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"a\": 1}"));

        var mediaType = await _tika.DetectAsync(stream, "data.json");

        Assert.That(mediaType, Is.EqualTo(MediaType.ApplicationJson));
    }

    // A context the caller made is the caller's: what one parse detects must not reach another parse that uses it.

    [Test]
    public async Task ParseAsync_ReusedContext_DoesNotCarryDetectedTypeIntoNextParse()
    {
        var context = new ParseContext();
        using (var wav = new MemoryStream(BuildWav()))
            await global::Tiki.TikiConfig.Default.Parser.ParseAsync(wav, context);

        using var mp3 = await OpenTestDataAsync("sample.mp3");
        var result = await new AudioParser().ParseAsync(mp3, context);

        Assert.That(context.Get<MediaType>(), Is.Null);
        Assert.That(result.MediaType, Is.EqualTo(MediaType.AudioMpeg));
        Assert.That(result.Title, Is.EqualTo("Sample Song"));
    }

    [Test]
    public async Task ParseAsync_SharedContextAcrossParallelParses_ParsesEach()
    {
        var mp3Bytes = await File.ReadAllBytesAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.mp3"));
        var wavBytes = BuildWav();
        var context = new ParseContext();
        var failures = 0;

        await Parallel.ForEachAsync(Enumerable.Range(0, 20_000), new ParallelOptions { MaxDegreeOfParallelism = 8 }, async (i, ct) =>
        {
            using var stream = new MemoryStream(i % 2 == 0 ? mp3Bytes : wavBytes);
            try
            {
                await global::Tiki.TikiConfig.Default.Parser.ParseAsync(stream, context, ct);
            }
            catch (ParseException)
            {
                Interlocked.Increment(ref failures);
            }
        });

        Assert.That(failures, Is.Zero);
    }

    // An ID3v2 tag says nothing about the audio after it, so that audio decides, not the name.

    [TestCase("song.flac")]
    [TestCase("song.m4a")]
    [TestCase("song.aac")]
    public async Task ParseAsync_Id3TaggedMp3NamedAsOtherAudio_ReturnsMp3(string fileName)
    {
        using var stream = await OpenTestDataAsync("sample.mp3");

        var result = await _tika.ParseAsync(stream, fileName);

        Assert.That(result, Is.InstanceOf<TikiMusic>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.AudioMpeg));
        Assert.That(result.Title, Is.EqualTo("Sample Song"));
    }

    [TestCase("song.flac")]
    [TestCase("song.mp3")]
    [TestCase("")]
    public async Task ParseAsync_Id3TaggedFlac_ReturnsFlac(string fileName)
    {
        using var stream = new MemoryStream(BuildId3TaggedFlac());

        var result = await _tika.ParseAsync(stream, fileName);

        Assert.That(result, Is.InstanceOf<TikiMusic>());
        Assert.That(result.MediaType, Is.EqualTo(MediaType.AudioFlac));
        Assert.That(result.Title, Is.EqualTo("Flac Song"));
    }

    // A WAV of a few silent 16-bit mono samples
    private static byte[] BuildWav()
    {
        var output = new MemoryStream();
        using (var writer = new BinaryWriter(output, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            const int dataLength = 16;
            writer.Write("RIFF"u8);
            writer.Write(36 + dataLength);
            writer.Write("WAVEfmt "u8);
            writer.Write(16);
            writer.Write((short)1);     // PCM
            writer.Write((short)1);     // mono
            writer.Write(8000);         // sample rate
            writer.Write(16000);        // byte rate
            writer.Write((short)2);     // block align
            writer.Write((short)16);    // bits per sample
            writer.Write("data"u8);
            writer.Write(dataLength);
            writer.Write(new byte[dataLength]);
        }
        return output.ToArray();
    }

    // An empty ID3v2.3 tag, then a FLAC stream with no audio frames: STREAMINFO and a Vorbis comment with a title
    private static byte[] BuildId3TaggedFlac()
    {
        var comment = "TITLE=Flac Song"u8.ToArray();
        var vendor = "Tiki"u8.ToArray();

        var output = new MemoryStream();
        using (var writer = new BinaryWriter(output, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            writer.Write(new byte[] { 0x49, 0x44, 0x33, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            writer.Write("fLaC"u8);

            // STREAMINFO: block size 4096, 44.1 kHz, stereo, 16-bit, no samples, no MD5
            writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x22 });
            writer.Write(new byte[] { 0x10, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0xC4, 0x42, 0xF0, 0x00, 0x00, 0x00, 0x00 });
            writer.Write(new byte[16]);

            // VORBIS_COMMENT, the last metadata block; its fields are little-endian
            var length = 4 + vendor.Length + 4 + 4 + comment.Length;
            writer.Write(new byte[] { 0x84, 0x00, 0x00, (byte)length });
            writer.Write(vendor.Length);
            writer.Write(vendor);
            writer.Write(1);
            writer.Write(comment.Length);
            writer.Write(comment);
        }
        return output.ToArray();
    }

    private static async Task<MemoryStream> OpenTestDataAsync(string name)
    {
        var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", name);
        return new MemoryStream(await File.ReadAllBytesAsync(filePath));
    }

    private static MemoryStream Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var deflate = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionMode.Compress, leaveOpen: true))
            deflate.Write(data, 0, data.Length);
        output.Position = 0;
        return output;
    }
}
