using AnimeFeedManager.Features.Tv.FeedManager.Collection;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.FeedManager.Queries;

/// <param name="Episode">Notified-episode identity: <c>05</c>, <c>05v2</c>, <c>12.5</c>.</param>
public sealed record RecentEpisode(string Episode, DateTimeOffset Published);

/// <param name="Latest">Newest <c>Published</c> among <paramref name="Episodes"/>.</param>
public sealed record RecentSeries(
    string SeriesId,
    string Title,
    DateTimeOffset Latest,
    ImmutableArray<RecentEpisode> Episodes);

internal sealed record SeriesCatalog(NyaaBindingStorage Binding, ImmutableArray<NyaaCatalogStorage> Catalog);

public static class RecentReleasesQuery
{
    /// <summary>Tracked series with releases inside the notification feed window, newest first; any unreadable catalog fails the query.</summary>
    public static Task<Result<ImmutableArray<RecentSeries>, DomainError>> GetRecentReleases(
        TrackedNyaaBindings trackedBindings,
        NyaaCatalogGetter catalogGetter,
        DateTimeOffset now,
        CancellationToken token) =>
        trackedBindings(token)
            .MapError(error => (DomainError)error)
            .Bind(tracked => ReadCatalogs(tracked, catalogGetter, token))
            .Map(catalogs => Summarize(catalogs, now - NyaaCollection.FeedWindow))
            .WithOperationName(nameof(GetRecentReleases));

    private static async Task<Result<ImmutableArray<SeriesCatalog>, DomainError>> ReadCatalogs(
        ImmutableArray<NyaaBindingStorage> tracked,
        NyaaCatalogGetter catalogGetter,
        CancellationToken token)
    {
        var reads = await Task.WhenAll(tracked.Select(binding =>
            catalogGetter(binding.RowKey ?? string.Empty, token).Map(catalog => new SeriesCatalog(binding, catalog))));

        return reads.Flatten(series => series.ToImmutableArray())
            .Bind(bulk => bulk.RequireAll("Some tracked series' catalogs could not be read"));
    }

    /// <summary>Same series selection as the notification feed; repeated episode labels keep the newest upload.</summary>
    internal static ImmutableArray<RecentSeries> Summarize(IEnumerable<SeriesCatalog> catalogs, DateTimeOffset since) =>
    [
        .. catalogs
            .Where(series => !string.IsNullOrWhiteSpace(series.Binding.FeedTitle))
            .Select(series => (series.Binding, Rows: Recent(series.Catalog, since)))
            .Where(series => !series.Rows.IsEmpty)
            .Select(series => new RecentSeries(
                series.Binding.RowKey ?? string.Empty,
                series.Binding.FeedTitle,
                series.Rows.Max(row => row.Published),
                [.. series.Rows.Select(row => new RecentEpisode(row.NotificationEpisode(), row.Published))]))
            .OrderByDescending(series => series.Latest)
    ];

    private static ImmutableArray<NyaaCatalogStorage> Recent(ImmutableArray<NyaaCatalogStorage> catalog,
        DateTimeOffset since) =>
    [
        .. catalog
            .Where(row => row.Published >= since)
            .GroupBy(row => row.NotificationEpisode())
            .Select(group => group.MaxBy(row => row.Published)!)
            .OrderByDescending(row => row.EpisodeNumber)
            .ThenByDescending(row => row.ReleaseVersion())
    ];
}
