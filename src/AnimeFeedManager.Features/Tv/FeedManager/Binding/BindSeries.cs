using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Binding;

public sealed record BoundSeries(string Title, bool WasOngoing);

public static class BindSeries
{
    /// <summary>Replaces the binding, feeds the library series from it, then queues the backfill; the catalog is kept.</summary>
    public static Task<Result<BoundSeries>> Bind(
        BindNyaaSeries request,
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        CancellationToken token) =>
        IdHelpers.SeriesSeasonFromId(request.SeriesId)
            .Bind(season => Bindable(request.SeriesId, season, clientFactory.TableStorageTvSeriesGetter, token)
                .Bind(series => clientFactory.TableStorageNyaaBindingReplacer(NewBinding(request, season, series), token))
                .Bind(_ => FeedLibrarySeries(request.SeriesId, season,
                    clientFactory.TableStorageTvSeriesGetter, clientFactory.TableStorageTvSeriesUpdater, token)))
            .Bind(bound => domainPostman(Events(request, bound), token).Map(_ => bound))
            .SendOnError(domainPostman,
                [
                    new SystemEvent(TargetConsumer.Admin(), EventTarget.Both, EventType.Error,
                        new NyaaBindFailed(request.SeriesId, request.Group, request.ReleaseTitle).AsEventPayload())
                ],
                token)
            .WithOperationName(nameof(Bind))
            .WithLogProperty(Telemetry.SeriesIdTag, request.SeriesId);

    private static Task<Result<AnimeInfoStorage>> Bindable(
        string seriesId,
        SeriesSeason season,
        TvSeriesGetter seriesGetter,
        CancellationToken token) =>
        seriesGetter(seriesId, season, token)
            .Bind(series => series.Feed is SubsPleaseFeed
                ? Error.Create($"{series.Title} is fed by SubsPlease and cannot be bound")
                : Result<AnimeInfoStorage>.Success(series));

    private static NyaaBindingStorage NewBinding(BindNyaaSeries request, SeriesSeason season, AnimeInfoStorage series) =>
        new()
        {
            PartitionKey = IdHelpers.GenerateAnimePartitionKey(season),
            RowKey = request.SeriesId,
            Group = request.Group,
            ReleaseTitle = request.ReleaseTitle,
            Variant = request.Variant,
            FeedTitle = series.Title ?? string.Empty,
            Trusted = request.Trusted,
            Tracking = true,
            Episodes = series.Episodes
        };

    private static Task<Result<BoundSeries>> FeedLibrarySeries(
        string seriesId,
        SeriesSeason season,
        TvSeriesGetter seriesGetter,
        TvSeriesStorageUpdater seriesUpdater,
        CancellationToken token) =>
        OptimisticConcurrency.UntilWritten(
            attemptToken => seriesGetter(seriesId, season, attemptToken)
                .Map(series => FeedFromBinding(series, seriesId))
                .Bind(fed => seriesUpdater(fed.Series, attemptToken).Map(_ => fed.Bound)),
            token,
            error => error is PreconditionFailedError);

    private static (AnimeInfoStorage Series, BoundSeries Bound) FeedFromBinding(AnimeInfoStorage series, string seriesId)
    {
        var bound = new BoundSeries(series.Title ?? string.Empty, series.Status == SeriesStatus.OngoingValue);
        series.FeedTitle = series.Title;
        series.FeedLink = FeedManagerRoutes.Releases(seriesId);
        series.FeedSource = SeriesFeeds.LocalSource;
        series.Status = SeriesStatus.Ongoing();
        return (series, bound);
    }

    private static DomainMessage[] Events(BindNyaaSeries request, BoundSeries bound) =>
    [
        new BackfillNyaaSeries(request.SeriesId),
        .. bound.WasOngoing ? Array.Empty<DomainMessage>() : [new UpdatedToOngoing(request.SeriesId, bound.Title)],
        new SystemEvent(TargetConsumer.Admin(), EventTarget.Both, EventType.Completed,
            new NyaaSeriesBound(request.SeriesId, bound.Title, request.Group, request.Variant).AsEventPayload())
    ];
}
