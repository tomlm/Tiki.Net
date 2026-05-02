using Tiki.Mime;

namespace Tiki.Detect;

/// <summary>
/// Combines multiple detectors, returning the first non-octet-stream result.
/// </summary>
public sealed class CompositeDetector : IDetector
{
    private readonly IDetector[] _detectors;

    public CompositeDetector(params IDetector[] detectors)
    {
        _detectors = detectors;
    }

    public CompositeDetector(IEnumerable<IDetector> detectors)
    {
        _detectors = detectors.ToArray();
    }

    public async Task<MediaType> DetectAsync(Stream? stream, string? fileName = null, CancellationToken cancellationToken = default)
    {
        // Remember stream position so each detector reads from the start
        var position = stream?.CanSeek == true ? stream.Position : -1;

        foreach (var detector in _detectors)
        {
            if (stream?.CanSeek == true)
                stream.Position = position;

            var result = await detector.DetectAsync(stream, fileName, cancellationToken).ConfigureAwait(false);
            if (result != MediaType.OctetStream)
                return result;
        }

        return MediaType.OctetStream;
    }
}
