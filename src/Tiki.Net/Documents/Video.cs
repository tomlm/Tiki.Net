namespace Tiki.Documents;

/// <summary>
/// Represents parsed metadata for video files (MP4, AVI, MKV, MOV, etc.).
/// </summary>
public class TikiVideo : TikiMedia
{
    /// <summary>
    /// The video frame width in pixels.
    /// </summary>
    public int? Width { get; init; }

    /// <summary>
    /// The video frame height in pixels.
    /// </summary>
    public int? Height { get; init; }

    /// <summary>
    /// The frame rate in frames per second.
    /// </summary>
    public double? FrameRate { get; init; }

    /// <summary>
    /// The video stream bitrate in bits per second.
    /// </summary>
    public int? VideoBitrate { get; init; }

    /// <summary>
    /// The video codec (e.g., H.264, VP9).
    /// </summary>
    public string? VideoCodec { get; init; }
}
