using System.Xml.Linq;
using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.FeedManager.Matching;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Collection;

/// <param name="Feeds">One feed per tracked series with a release published in the last <see cref="NyaaCollection.FeedWindow"/>.</param>
/// <param name="Catalogued">Rows matched this run, including re-polled ones.</param>
/// <param name="PolledAt">When the poll was requested; null when nothing was tracked, so nyaa wasn't polled.</param>
/// <param name="OldestPolled">Oldest <c>Published</c> among the polled items; null when none were.</param>
/// <param name="TrackedSeries">Series ids of every tracked binding.</param>
public sealed record NyaaCollected(
    ImmutableArray<DailySeriesFeed> Feeds,
    int Catalogued,
    DateTimeOffset? PolledAt,
    DateTimeOffset? OldestPolled,
    ImmutableArray<string> TrackedSeries);

internal sealed record SeriesMatch(NyaaBindingStorage Binding, ImmutableArray<NyaaCatalogStorage> Rows);

/// <param name="Matched">Info hashes matched this run.</param>
internal sealed record CollectedSeries(
    NyaaBindingStorage Binding,
    ImmutableArray<NyaaCatalogStorage> Catalog,
    ImmutableHashSet<string> Matched);

internal sealed record CollectionProcess(ImmutableArray<NyaaBindingStorage> Tracked)
{
    public DateTimeOffset? PolledAt { get; init; }
    public DateTimeOffset? OldestPolled { get; init; }
    public ImmutableArray<SeriesMatch> Matches { get; init; } = [];
    public ImmutableArray<CollectedSeries> Collected { get; init; } = [];
}

public static class NyaaCollection
{
    internal const int FeedEpisodes = 10;

    // Every run re-offers the day's releases, so one missed by a run, or recovered by a backfill, still goes out;
    // users' notified episodes keep it from going out twice.
    public static readonly TimeSpan FeedWindow = TimeSpan.FromHours(24);

    // Unfiltered so one request covers every tracked binding; matching applies each binding's rules.
    private static readonly NyaaQuery PollQuery = NyaaQuery.Create().Search("1080");

    /// <summary>Polls nyaa once, catalogs every tracked binding's matches, then builds feeds from the tracked catalogs; any failure fails the run.</summary>
    public static Task<Result<NyaaCollected>> Collect(
        INyaaClient nyaaClient,
        ITableClientFactory clientFactory,
        WebAppHost host,
        CancellationToken token) =>
        clientFactory.TableStorageTrackedNyaaBindings(token)
            .Map(tracked => new CollectionProcess(tracked))
            .PollReleases(nyaaClient, token)
            .StoreReleases(clientFactory.TableStorageNyaaCatalogUpdater, token)
            .ReadCatalogs(clientFactory.TableStorageNyaaCatalogGetter, token)
            .Map(p => new NyaaCollected(
                Feeds(p.Collected, host, (p.PolledAt ?? DateTimeOffset.UtcNow) - FeedWindow),
                p.Matches.Sum(match => match.Rows.Length),
                p.PolledAt,
                p.OldestPolled,
                [.. p.Tracked.Select(binding => binding.RowKey ?? string.Empty)]))
            .WithOperationName(nameof(Collect));

    extension(Task<Result<CollectionProcess>> process)
    {
        private Task<Result<CollectionProcess>> PollReleases(INyaaClient nyaaClient, CancellationToken token) =>
            process.Bind(p => p.Tracked.IsEmpty
                ? Task.FromResult(Result<CollectionProcess>.Success(p))
                : Poll(p with {PolledAt = DateTimeOffset.UtcNow}, nyaaClient, token));

        private Task<Result<CollectionProcess>> StoreReleases(NyaaCatalogUpdater catalogUpdater,
            CancellationToken token) =>
            process.Bind(p => catalogUpdater(p.Matches.SelectMany(match => match.Rows), token).Map(_ => p));

        private Task<Result<CollectionProcess>> ReadCatalogs(NyaaCatalogGetter catalogGetter,
            CancellationToken token) =>
            process.Bind(async p =>
            {
                var matched = p.Matches.ToDictionary(match => match.Binding.RowKey ?? string.Empty,
                    match => match.Rows.Select(row => row.InfoHash).ToImmutableHashSet());

                return (await Task.WhenAll(p.Tracked.Select(binding =>
                        ReadCatalog(binding, matched.GetValueOrDefault(binding.RowKey ?? string.Empty, []),
                            catalogGetter, token))))
                    .Flatten(series => series.ToImmutableArray())
                    .Bind(bulk => bulk.RequireAll("Some tracked series' catalogs could not be read"))
                    .Map(collected => p with {Collected = collected});
            });
    }

    private static Task<Result<CollectionProcess>> Poll(CollectionProcess process, INyaaClient nyaaClient,
        CancellationToken token) =>
        nyaaClient.Fetch(PollQuery, token).Bind(document => MatchReleases(process, document));

    private static Result<CollectionProcess> MatchReleases(CollectionProcess process, XDocument document) =>
        NyaaFeed.Parse(document)
            .AddLogOnSuccess(feed => feed.LogErrors)
            .Map(feed => process with
            {
                Matches = Match(process.Tracked, feed.Value),
                OldestPolled = feed.Value.IsEmpty ? null : feed.Value.Min(item => item.Published)
            });

    private static Task<Result<CollectedSeries>> ReadCatalog(NyaaBindingStorage binding,
        ImmutableHashSet<string> matched, NyaaCatalogGetter catalogGetter, CancellationToken token) =>
        catalogGetter(binding.RowKey ?? string.Empty, token)
            .Map(catalog => new CollectedSeries(binding, catalog, matched));

    /// <summary>Each tracked binding's catalog rows among the polled items; bindings without any are left out.</summary>
    internal static ImmutableArray<SeriesMatch> Match(ImmutableArray<NyaaBindingStorage> tracked,
        ImmutableArray<NyaaItem> items) =>
    [
        .. tracked
            .Select(binding => new SeriesMatch(binding,
                NyaaMatch.CatalogRows(binding, binding.RowKey ?? string.Empty, items)))
            .Where(match => !match.Rows.IsEmpty)
    ];

    /// <summary>Series with a release published since <paramref name="since"/>; one feed per feed title, from the newest season among the series sharing it.</summary>
    internal static ImmutableArray<DailySeriesFeed> Feeds(IEnumerable<CollectedSeries> collected, WebAppHost host,
        DateTimeOffset since) =>
    [
        .. collected
            .Where(series => !string.IsNullOrWhiteSpace(series.Binding.FeedTitle))
            .Where(series => series.Catalog.Any(row => row.Published >= since))
            .GroupBy(series => series.Binding.FeedTitle)
            .Select(group => group.MaxBy(series => SeasonOrder(series.Binding))!)
            .Select(series => Feed(series, host))
    ];

    // Latest episodes first; a repeated episode string keeps its newest upload.
    private static DailySeriesFeed Feed(CollectedSeries series, WebAppHost host) =>
        new(series.Binding.FeedTitle,
            host.Absolute(FeedManagerRoutes.Releases(series.Binding.RowKey ?? string.Empty)),
            [
                .. series.Catalog
                    .GroupBy(row => row.NotificationEpisode())
                    .Select(group => group.MaxBy(row => row.Published)!)
                    .OrderByDescending(row => row.EpisodeNumber)
                    .ThenByDescending(row => row.ReleaseVersion())
                    .Take(FeedEpisodes)
                    .Select(row => new EpisodeData(row.NotificationEpisode(), row.Magnet, row.Torrent,
                        series.Matched.Contains(row.InfoHash)))
            ]);

    // An unparsable id sorts oldest.
    private static (Year Year, Season Season) SeasonOrder(NyaaBindingStorage binding) =>
        IdHelpers.SeriesSeasonFromId(binding.RowKey ?? string.Empty).MatchToValue(
            season => (season.Year, season.Season),
            _ => default);
}
