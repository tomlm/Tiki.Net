namespace Tiki.IO;

/// <summary>
/// A stream wrapper that ensures seekability by buffering non-seekable streams.
/// Used for detection and parsing where the stream needs to be read multiple times.
/// </summary>
public sealed class TikiStream : Stream, IAsyncDisposable
{
    private readonly Stream _innerStream;
    private readonly bool _ownsStream;

    private TikiStream(Stream innerStream, bool ownsStream)
    {
        _innerStream = innerStream;
        _ownsStream = ownsStream;
    }

    /// <summary>
    /// Creates a TikiStream from the given stream. If the stream is not seekable,
    /// it will be copied to a MemoryStream first.
    /// </summary>
    public static async Task<TikiStream> CreateAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream.CanSeek)
            return new TikiStream(stream, ownsStream: false);

        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        memoryStream.Position = 0;
        return new TikiStream(memoryStream, ownsStream: true);
    }

    /// <summary>
    /// Creates a TikiStream from a file path.
    /// </summary>
    public static TikiStream Create(string filePath)
    {
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new TikiStream(fileStream, ownsStream: true);
    }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    public override void Flush() => _innerStream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);
    public override int Read(Span<byte> buffer) => _innerStream.Read(buffer);
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _innerStream.ReadAsync(buffer, cancellationToken);
    public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing && _ownsStream)
            _innerStream.Dispose();
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_ownsStream)
            await _innerStream.DisposeAsync().ConfigureAwait(false);
    }
}
