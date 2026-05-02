namespace Tiki.Documents;

/// <summary>
/// Represents parsed metadata for music files with artist/album/track tags.
/// </summary>
public class TikiMusic : TikiAudio
{
    /// <summary>
    /// The performing artist.
    /// </summary>
    public string? Artist { get; init; }

    /// <summary>
    /// The album artist (may differ from track artist on compilations).
    /// </summary>
    public string? AlbumArtist { get; init; }

    /// <summary>
    /// The album title.
    /// </summary>
    public string? Album { get; init; }

    /// <summary>
    /// The genre(s) of the track.
    /// </summary>
    public string[]? Genre { get; init; }

    /// <summary>
    /// The track number on the album.
    /// </summary>
    public int? TrackNumber { get; init; }

    /// <summary>
    /// The disc number in a multi-disc set.
    /// </summary>
    public int? DiscNumber { get; init; }

    /// <summary>
    /// The release year.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// The composer of the track.
    /// </summary>
    public string? Composer { get; init; }
}
