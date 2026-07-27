namespace AnimeFeedManager.Features.Library.Events;

public static class SeasonImportedExtensions
{
    /// <summary>"12 series imported for Spring 2026 (8 TV, 3 Movie, 1 OVA)" — for LibraryEvent.Summary.</summary>
    public static string ToSummary(this SeasonImported evt)
    {
        var total = evt.ByType.Sum(count => count.Count);
        var breakdown = string.Join(", ", evt.ByType.Select(count => $"{count.Count} {count.TypeLabel}"));
        return $"{total} series imported for {evt.Season.ToDisplayLabel()} ({breakdown})";
    }
}
