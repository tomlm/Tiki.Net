using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using Tiki.Documents;
using Tiki.Mime;
using Tiki.Parser;

namespace Tiki.Parsers.Image;

/// <summary>
/// Parser for image files that extracts EXIF, IPTC, and XMP metadata.
/// </summary>
public sealed class ImageMetadataParser : AbstractParser
{
    private static readonly HashSet<MediaType> s_supportedTypes = new()
    {
        MediaType.ImageJpeg,
        MediaType.ImagePng,
        MediaType.ImageTiff,
        MediaType.ImageGif,
        MediaType.ImageBmp,
        MediaType.ImageWebp
    };

    public override IReadOnlySet<MediaType> SupportedTypes => s_supportedTypes;

    public override Task<Documents.TikiFile> ParseAsync(Stream stream, ParseContext? context = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(stream);

            string? cameraManufacturer = null;
            string? cameraModel = null;
            string? lensModel = null;
            DateTime? dateTaken = null;
            double? exposureTime = null;
            double? fNumber = null;
            int? isoSpeed = null;
            double? focalLength = null;
            int? orientation = null;
            int? width = null;
            int? height = null;
            double? latitude = null;
            double? longitude = null;
            string? flash = null;
            string? whiteBalance = null;
            string? meteringMode = null;
            double? exposureBias = null;
            string? title = null;
            string? description = null;
            string[]? keywords = null;

            foreach (var directory in directories)
            {
                if (directory is ExifIfd0Directory ifd0)
                {
                    cameraManufacturer = GetString(ifd0, ExifDirectoryBase.TagMake);
                    cameraModel = GetString(ifd0, ExifDirectoryBase.TagModel);
                    orientation = GetInt(ifd0, ExifDirectoryBase.TagOrientation);
                    title = GetString(ifd0, ExifDirectoryBase.TagImageDescription);

                    if (ifd0.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var dt))
                        dateTaken ??= dt;
                }
                else if (directory is ExifSubIfdDirectory subIfd)
                {
                    if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dto))
                        dateTaken = dto;

                    exposureTime = GetRationalAsDouble(subIfd, ExifDirectoryBase.TagExposureTime);
                    fNumber = GetRationalAsDouble(subIfd, ExifDirectoryBase.TagFNumber);
                    focalLength = GetRationalAsDouble(subIfd, ExifDirectoryBase.TagFocalLength);
                    exposureBias = GetRationalAsDouble(subIfd, ExifDirectoryBase.TagExposureBias);

                    isoSpeed = GetInt(subIfd, ExifDirectoryBase.TagIsoEquivalent);
                    flash = GetString(subIfd, ExifDirectoryBase.TagFlash);
                    whiteBalance = GetString(subIfd, ExifDirectoryBase.TagWhiteBalance);
                    meteringMode = GetString(subIfd, ExifDirectoryBase.TagMeteringMode);
                    lensModel = GetString(subIfd, ExifDirectoryBase.TagLensModel);
                }
                else if (directory is GpsDirectory gps)
                {
                    var location = gps.GetGeoLocation();
                    if (location != null)
                    {
                        latitude = location.Latitude;
                        longitude = location.Longitude;
                    }
                }
                else if (directory is JpegDirectory jpeg)
                {
                    width ??= GetInt(jpeg, JpegDirectory.TagImageWidth);
                    height ??= GetInt(jpeg, JpegDirectory.TagImageHeight);
                }
                else if (directory is PngDirectory png)
                {
                    width ??= GetInt(png, PngDirectory.TagImageWidth);
                    height ??= GetInt(png, PngDirectory.TagImageHeight);
                }
                else if (directory is IptcDirectory iptc)
                {
                    title ??= GetString(iptc, IptcDirectory.TagObjectName);
                    description ??= GetString(iptc, IptcDirectory.TagCaption);
                    var kw = GetString(iptc, IptcDirectory.TagKeywords);
                    if (kw != null)
                        keywords = kw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }
            }

            // Detect media type based on file content
            var mediaType = DetectImageType(directories);

            return Task.FromResult<Documents.TikiFile>(new TikiPhoto
            {
                Content = string.Empty,
                MediaType = mediaType,
                Title = title,
                Description = description,
                Keywords = keywords,
                DateCreated = dateTaken,
                CameraManufacturer = cameraManufacturer,
                CameraModel = cameraModel,
                LensModel = lensModel,
                DateTaken = dateTaken,
                ExposureTime = exposureTime,
                FNumber = fNumber,
                IsoSpeed = isoSpeed,
                FocalLength = focalLength,
                Orientation = orientation,
                Width = width,
                Height = height,
                Latitude = latitude,
                Longitude = longitude,
                Flash = flash,
                WhiteBalance = whiteBalance,
                MeteringMode = meteringMode,
                ExposureBias = exposureBias,
                ContentLength = stream.CanSeek ? stream.Length : null
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new ParseException($"Failed to parse image metadata: {ex.Message}", ex);
        }
    }

    private static MediaType DetectImageType(IReadOnlyList<MetadataExtractor.Directory> directories)
    {
        foreach (var dir in directories)
        {
            if (dir is JpegDirectory) return MediaType.ImageJpeg;
            if (dir is PngDirectory) return MediaType.ImagePng;
        }

        if (directories.Any(d => d.Name.Contains("TIFF", StringComparison.OrdinalIgnoreCase)))
            return MediaType.ImageTiff;

        return MediaType.ImageJpeg;
    }

    private static string? GetString(MetadataExtractor.Directory dir, int tagType)
    {
        var value = dir.GetDescription(tagType);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int? GetInt(MetadataExtractor.Directory dir, int tagType)
    {
        return dir.TryGetInt32(tagType, out var value) ? value : null;
    }

    private static double? GetRationalAsDouble(MetadataExtractor.Directory dir, int tagType)
    {
        if (dir.TryGetRational(tagType, out var rational))
            return rational.ToDouble();
        return null;
    }
}
