using Tiki.Mime;

namespace Tiki.Detect;

/// <summary>
/// Detects the media type of a document.
/// </summary>
public interface IDetector
{
    /// <summary>
    /// Detects the media type of the given stream.
    /// </summary>
    /// <param name="stream">The document stream (may be null for filename-only detection).</param>
    /// <param name="fileName">Optional file name hint for extension-based detection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The detected media type, or OctetStream if unknown.</returns>
    Task<MediaType> DetectAsync(
        Stream? stream,
        string? fileName = null,
        CancellationToken cancellationToken = default);
}
