namespace Tiki.Documents;

/// <summary>
/// Abstract base class for all media types (audio and video).
/// Contains properties common to both.
/// </summary>
public abstract class TikiMedia : TikiFile
{
    /// <summary>
    /// The duration of the media.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// The overall bitrate in bits per second.
    /// </summary>
    public int? Bitrate { get; init; }

    /// <summary>
    /// The audio sample rate in Hz.
    /// </summary>
    public int? SampleRate { get; init; }

    /// <summary>
    /// The number of audio channels.
    /// </summary>
    public int? Channels { get; init; }

    /// <summary>
    /// The codec used for encoding.
    /// </summary>
    public string? Codec { get; init; }
}
