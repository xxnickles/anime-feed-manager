using System.Xml.Linq;
using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.FeedManager.Matching;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Collection;

/// <param name="Feeds">One feed per series matched this run.</param>
/// <param name="Catalogued">Rows matched this run, including re-polled ones.</param>
/// <param name="PolledAt">When the poll was requested; null when nothing was tracked, so nyaa wasn't polled.</param>
public sealed record NyaaCollected(ImmutableArray<DailySeriesFeed> Feeds, int Catalogued, DateTimeOffset? PolledAt);

internal sealed record SeriesMatch(NyaaBindingStorage Binding, ImmutableArray<NyaaCatalogStorage> Rows);

/// <param name="Matched">Info hashes matched this run.</param>
internal sealed record CollectedSeries(
    NyaaBindingStorage Binding,
    ImmutableArray<NyaaCatalogStorage> Catalog,
    ImmutableHashSet<string> Matched);

internal sealed record CollectionProcess(ImmutableArray<NyaaBindingStorage> Tracked)
{
    public DateTimeOffset? PolledAt { get; init; }
    public ImmutableArray<SeriesMatch> Matches { get; init; } = [];
    public ImmutableArray<CollectedSeries> Collected { get; init; } = [];
}

public static class NyaaCollection
{
    internal const int FeedEpisodes = 10;

    // Unfiltered so one request covers every tracked binding; matching applies each binding's rules.
    private static readonly NyaaQuery PollQuery = NyaaQuery.Create().Search("1080");

    /// <summary>Polls nyaa once, catalogs every tracked binding's matches, and builds a feed per matched series from its catalog; any failure fails the run.</summary>
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
            .Map(p => new NyaaCollected(Feeds(p.Collected, host), p.Matches.Sum(match => match.Rows.Length), p.PolledAt))
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
                (await Task.WhenAll(p.Matches.Select(match => ReadCatalog(match, catalogGetter, token))))
                .Flatten(series => series.ToImmutableArray())
                .Bind(bulk => bulk.RequireAll("Some matched series' catalogs could not be read"))
                .Map(collected => p with {Collected = collected}));
    }

    private static Task<Result<CollectionProcess>> Poll(CollectionProcess process, INyaaClient nyaaClient,
        CancellationToken token) =>
        nyaaClient.Fetch(PollQuery, token).Bind(document => MatchReleases(process, document));

    private static Result<CollectionProcess> MatchReleases(CollectionProcess process, XDocument document) =>
        NyaaFeed.Parse(document)
            .AddLogOnSuccess(feed => feed.LogErrors)
            .Map(feed => process with {Matches = Match(process.Tracked, feed.Value)});

    private static Task<Result<CollectedSeries>> ReadCatalog(SeriesMatch match, NyaaCatalogGetter catalogGetter,
        CancellationToken token) =>
        catalogGetter(match.Binding.RowKey ?? string.Empty, token)
            .Map(catalog => new CollectedSeries(match.Binding, catalog,
                [.. match.Rows.Select(row => row.InfoHash)]));

    /// <summary>Each tracked binding's catalog rows among the polled items; bindings without any are left out.</summary>
    internal static ImmutableArray<SeriesMatch> Match(ImmutableArray<NyaaBindingStorage> tracked,
        ImmutableArray<NyaaItem> items) =>
    [
        .. tracked
            .Select(binding => new SeriesMatch(binding,
                NyaaMatch.CatalogRows(binding, binding.RowKey ?? string.Empty, items)))
            .Where(match => !match.Rows.IsEmpty)
    ];

    /// <summary>One feed per feed title, from the newest season among the series sharing it.</summary>
    internal static ImmutableArray<DailySeriesFeed> Feeds(IEnumerable<CollectedSeries> collected, WebAppHost host) =>
    [
        .. collected
            .Where(series => !string.IsNullOrWhiteSpace(series.Binding.FeedTitle))
            .GroupBy(series => series.Binding.FeedTitle)
            .Select(group => group.MaxBy(series => SeasonOrder(series.Binding))!)
            .Select(series => Feed(series, host))
            .Where(feed => feed.Episodes.Length > 0)
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
