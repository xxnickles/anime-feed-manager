using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tv.FeedManager.SeriesReleases;

/// <param name="View">nyaa's page for the release; null when <paramref name="Torrent"/> is not nyaa's download URL.</param>
public sealed record SeriesRelease(
    string Title,
    int Version,
    DateTimeOffset Published,
    string Size,
    string Magnet,
    string Torrent,
    Uri? View);

/// <param name="Earlier">Superseded releases of the episode, highest version first.</param>
public sealed record EpisodeReleases(
    string Label,
    bool Special,
    SeriesRelease Latest,
    ImmutableArray<SeriesRelease> Earlier);

public abstract record ReleaseCadence;

/// <summary>UTC weekday and time, rounded to the half hour.</summary>
public sealed record WeeklyCadence(DayOfWeek Day, TimeOnly Time) : ReleaseCadence;

public sealed record LastRelease(DateTimeOffset Published) : ReleaseCadence;

public sealed record NoReleases : ReleaseCadence;

public static class NyaaReleases
{
    private const int CadenceEpisodes = 4;
    private const int MinimumCadenceEpisodes = 3;
    private static readonly TimeSpan WeeklyMin = TimeSpan.FromDays(6);
    private static readonly TimeSpan WeeklyMax = TimeSpan.FromDays(8);
    private static readonly TimeSpan SlotRounding = TimeSpan.FromMinutes(30);
    private const string TorrentExtension = ".torrent";

    /// <summary>One entry per episode, newest episode first; the highest version is the latest, ties to the newest upload.</summary>
    public static ImmutableArray<EpisodeReleases> ByEpisode(IEnumerable<NyaaCatalogStorage> catalog) =>
    [
        ..catalog
            .GroupBy(row => (row.EpisodeNumber, row.Special))
            .OrderByDescending(episode => episode.Key.EpisodeNumber)
            .ThenBy(episode => episode.Key.Special)
            .Select(ToEpisode)
    ];

    /// <summary>Weekly when the first releases of the last regular episodes are a median 6–8 days apart; otherwise the newest release.</summary>
    public static ReleaseCadence Cadence(ImmutableArray<NyaaCatalogStorage> catalog)
    {
        var firstReleases = RecentFirstReleases(catalog);
        return catalog.IsEmpty ? new NoReleases()
            : IsWeekly(firstReleases) ? Slot(firstReleases[^1])
            : new LastRelease(catalog.Max(row => row.Published));
    }

    /// <summary>nyaa's page for a release, from its <c>/download/{id}.torrent</c> URL.</summary>
    public static Uri? ViewPage(string torrent) =>
        Uri.TryCreate(torrent, UriKind.Absolute, out var url)
        && url.Segments is [_, "download/", var file]
        && file.EndsWith(TorrentExtension, StringComparison.Ordinal)
        && file[..^TorrentExtension.Length] is {Length: > 0} id
        && id.All(char.IsAsciiDigit)
            ? new Uri(url, $"/view/{id}")
            : null;

    private static EpisodeReleases ToEpisode(IGrouping<(double EpisodeNumber, bool Special), NyaaCatalogStorage> episode)
    {
        var rows = episode
            .OrderByDescending(row => row.ReleaseVersion())
            .ThenByDescending(row => row.Published)
            .ToImmutableArray();
        return new EpisodeReleases(
            rows[0].EpisodeLabel,
            episode.Key.Special,
            ToRelease(rows[0]),
            [..rows.Skip(1).Select(ToRelease)]);
    }

    private static SeriesRelease ToRelease(NyaaCatalogStorage row) =>
        new(row.Title, row.ReleaseVersion(), row.Published, row.Size, row.Magnet, row.Torrent, ViewPage(row.Torrent));

    // Revisions are excluded: each regular episode counts at its first release.
    private static ImmutableArray<DateTimeOffset> RecentFirstReleases(IEnumerable<NyaaCatalogStorage> catalog) =>
    [
        ..catalog
            .Where(row => !row.Special)
            .GroupBy(row => row.EpisodeNumber)
            .OrderByDescending(episode => episode.Key)
            .Take(CadenceEpisodes)
            .Select(episode => episode.Min(row => row.Published))
            .Order()
    ];

    private static bool IsWeekly(ImmutableArray<DateTimeOffset> firstReleases) =>
        firstReleases.Length >= MinimumCadenceEpisodes
        && MedianGap(firstReleases) is var gap
        && gap >= WeeklyMin
        && gap <= WeeklyMax;

    private static TimeSpan MedianGap(ImmutableArray<DateTimeOffset> ordered)
    {
        var gaps = ordered.Zip(ordered.Skip(1), (earlier, later) => later - earlier).Order().ToImmutableArray();
        var middle = gaps.Length / 2;
        return gaps.Length % 2 == 1 ? gaps[middle] : (gaps[middle - 1] + gaps[middle]) / 2;
    }

    private static WeeklyCadence Slot(DateTimeOffset firstRelease)
    {
        var utc = firstRelease.UtcDateTime;
        var rounded = new DateTime(
            (utc.Ticks + SlotRounding.Ticks / 2) / SlotRounding.Ticks * SlotRounding.Ticks,
            DateTimeKind.Utc);
        return new WeeklyCadence(rounded.DayOfWeek, TimeOnly.FromDateTime(rounded));
    }
}
