namespace Tiki.Documents;

/// <summary>
/// Represents parsed metadata for presentation documents (PPTX, PPT).
/// </summary>
public class TikiPresentation : TikiOfficeDocument
{
    /// <summary>
    /// The number of slides in the presentation.
    /// </summary>
    public int? SlideCount { get; init; }
}
