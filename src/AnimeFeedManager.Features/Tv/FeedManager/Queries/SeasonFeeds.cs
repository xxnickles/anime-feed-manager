using AnimeFeedManager.Features.Tv.FeedManager.Staleness;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Queries;

public abstract record SeasonFeed(string SeriesId, string Title);

public sealed record UnfedSeries(string SeriesId, string Title, ExpectedEpisodes Episodes)
    : SeasonFeed(SeriesId, Title);

public sealed record BoundSeries(
    string SeriesId,
    string Title,
    string Group,
    int LatestEpisode,
    ExpectedEpisodes Episodes,
    BindingState State) : SeasonFeed(SeriesId, Title);

public enum BindingState
{
    Tracking,
    Stale,
    Completed
}

public static class SeasonFeeds
{
    /// <summary>Unfed, uncompleted library series plus the season's bindings; stale first, then unfed, tracking, completed.</summary>
    public static Task<Result<ImmutableArray<SeasonFeed>>> GetSeasonFeeds(
        SeriesSeason season,
        Uri publicBlobUri,
        TvLibrary libraryGetter,
        SeasonNyaaBindings bindingsGetter,
        NyaaCatalogGetter catalogGetter,
        DateTimeOffset now,
        CancellationToken token) =>
        bindingsGetter(season, token)
            .Bind(bindings => Bound(bindings, catalogGetter, now, token))
            .Bind(bound => libraryGetter(season, publicBlobUri, token)
                .Map(library => Ordered([..Unfed(library, bound), ..bound])))
            .WithOperationName(nameof(GetSeasonFeeds))
            .WithLogProperty(Telemetry.SeriesSeasonTag, IdHelpers.GenerateAnimePartitionKey(season));

    private static async Task<Result<ImmutableArray<BoundSeries>>> Bound(
        ImmutableArray<NyaaBindingStorage> bindings,
        NyaaCatalogGetter catalogGetter,
        DateTimeOffset now,
        CancellationToken token)
    {
        var reads = await Task.WhenAll(bindings.Select(binding =>
            catalogGetter(binding.RowKey ?? string.Empty, token).Map(catalog => ToBound(binding, catalog, now))));

        return reads.Flatten(rows => rows.ToImmutableArray()).Bind(AllRead);
    }

    // A bound series missing from the list would read as never bound.
    private static Result<ImmutableArray<BoundSeries>> AllRead(BulkResult<ImmutableArray<BoundSeries>> bulk) =>
        bulk is PartialSuccessBulkResult<ImmutableArray<BoundSeries>> partial
            ? new AggregatedError("Some binding catalogs could not be read", partial.Errors)
            : bulk.Value;

    private static BoundSeries ToBound(
        NyaaBindingStorage binding,
        ImmutableArray<NyaaCatalogStorage> catalog,
        DateTimeOffset now) =>
        new(binding.RowKey ?? string.Empty,
            binding.FeedTitle,
            binding.Group,
            catalog.LatestRegular(),
            SeriesBroadcast.ToExpectedEpisodes(binding.Episodes),
            binding switch
            {
                {CompletedAt: not null} => BindingState.Completed,
                _ when NyaaStaleness.IsStale(binding, catalog, now) => BindingState.Stale,
                _ => BindingState.Tracking
            });

    private static IEnumerable<UnfedSeries> Unfed(ImmutableArray<TvSeries> library, ImmutableArray<BoundSeries> bound) =>
        library
            .Where(series => string.IsNullOrWhiteSpace(series.FeedTitle)
                             && series.Status != SeriesStatus.Completed()
                             && bound.All(b => b.SeriesId != series.Id))
            .Select(series => new UnfedSeries(series.Id, series.Title, series.Episodes));

    private static ImmutableArray<SeasonFeed> Ordered(IEnumerable<SeasonFeed> feeds) =>
    [
        ..feeds.OrderBy(feed => feed switch
            {
                BoundSeries {State: BindingState.Stale} => 0,
                UnfedSeries => 1,
                BoundSeries {State: BindingState.Tracking} => 2,
                _ => 3
            })
            .ThenBy(feed => feed.Title, StringComparer.OrdinalIgnoreCase)
    ];
}
