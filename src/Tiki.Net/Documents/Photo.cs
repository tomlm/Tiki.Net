namespace Tiki.Documents;

/// <summary>
/// Represents parsed metadata for image files (JPEG, PNG, TIFF, etc.).
/// </summary>
public class TikiPhoto : TikiFile
{
    /// <summary>
    /// The camera manufacturer (e.g., "Canon", "Nikon").
    /// </summary>
    public string? CameraManufacturer { get; init; }

    /// <summary>
    /// The camera model (e.g., "EOS R5").
    /// </summary>
    public string? CameraModel { get; init; }

    /// <summary>
    /// The lens model used.
    /// </summary>
    public string? LensModel { get; init; }

    /// <summary>
    /// The date and time the photo was taken.
    /// </summary>
    public DateTime? DateTaken { get; init; }

    /// <summary>
    /// The exposure time (shutter speed) in seconds.
    /// </summary>
    public double? ExposureTime { get; init; }

    /// <summary>
    /// The f-number (aperture).
    /// </summary>
    public double? FNumber { get; init; }

    /// <summary>
    /// The ISO speed rating.
    /// </summary>
    public int? IsoSpeed { get; init; }

    /// <summary>
    /// The focal length in millimeters.
    /// </summary>
    public double? FocalLength { get; init; }

    /// <summary>
    /// The EXIF orientation value (1-8).
    /// </summary>
    public int? Orientation { get; init; }

    /// <summary>
    /// The image width in pixels.
    /// </summary>
    public int? Width { get; init; }

    /// <summary>
    /// The image height in pixels.
    /// </summary>
    public int? Height { get; init; }

    /// <summary>
    /// The GPS latitude in decimal degrees.
    /// </summary>
    public double? Latitude { get; init; }

    /// <summary>
    /// The GPS longitude in decimal degrees.
    /// </summary>
    public double? Longitude { get; init; }

    /// <summary>
    /// The flash mode/status.
    /// </summary>
    public string? Flash { get; init; }

    /// <summary>
    /// The white balance setting.
    /// </summary>
    public string? WhiteBalance { get; init; }

    /// <summary>
    /// The metering mode used.
    /// </summary>
    public string? MeteringMode { get; init; }

    /// <summary>
    /// The exposure bias/compensation in EV.
    /// </summary>
    public double? ExposureBias { get; init; }
}
