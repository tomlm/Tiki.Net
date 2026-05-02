using TagLib;

namespace Tiki.Parsers.Media;

/// <summary>
/// TagLib IFileAbstraction implementation that wraps a Stream.
/// </summary>
internal sealed class StreamFileAbstraction : TagLib.File.IFileAbstraction
{
    private readonly Stream _stream;

    public StreamFileAbstraction(string name, Stream stream)
    {
        Name = name;
        _stream = stream;
    }

    public string Name { get; }

    public Stream ReadStream => _stream;

    public Stream WriteStream => _stream;

    public void CloseStream(Stream stream)
    {
        // Don't close the stream - we don't own it
    }
}
