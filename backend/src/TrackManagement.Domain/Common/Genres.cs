namespace TrackManagement.Domain.Common;

/// <summary>
/// The genres a track can have. Keeping one fixed list (instead of free text) stops
/// near-duplicates like "Hip Hop" / "hip-hop" / "HipHop", which would break the genre filter.
/// The API exposes this list (GET /api/genres) so the UI dropdown and the validation always agree.
/// </summary>
public static class Genres
{
    public static readonly IReadOnlyList<string> All =
    [
        "Arabic Pop",
        "Classical",
        "Electronic",
        "Hip Hop",
        "Indie",
        "Jazz",
        "Khaleeji",
        "Mahraganat",
        "Pop",
        "R&B",
        "Rai",
        "Rock",
        "Shaabi",
        "Tarab"
    ];

    /// <summary>
    /// The genre as written in <see cref="All"/> ("hip hop" -> "Hip Hop"), or null if it is not a known genre.
    /// </summary>
    public static string? ToCanonical(string? genre) =>
        string.IsNullOrWhiteSpace(genre)
            ? null
            : All.FirstOrDefault(g => string.Equals(g, genre.Trim(), StringComparison.OrdinalIgnoreCase));

    public static bool IsKnown(string? genre) => ToCanonical(genre) is not null;
}
