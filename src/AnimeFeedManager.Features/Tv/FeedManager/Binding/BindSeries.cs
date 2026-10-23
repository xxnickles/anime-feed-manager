using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Binding;

public sealed record BoundSeries(string Title, bool WasOngoing);

public static class BindSeries
{
    /// <summary>
    /// Replaces the binding, wipes the catalog, feeds the library series from it, then queues the backfill.
    /// The binding is written before the wipe so an hourly run in between can only add rows the backfill restores.
    /// </summary>
    public static Task<Result<BoundSeries>> Bind(
        BindNyaaSeries request,
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        CancellationToken token) =>
        IdHelpers.SeriesSeasonFromId(request.SeriesId)
            .Bind(season => Bindable(request.SeriesId, season,
                    clientFactory.TableStorageTvSeriesGetter, clientFactory.TableStorageNyaaBindingGetter, token)
                .Bind(series => clientFactory.TableStorageNyaaBindingReplacer(NewBinding(request, season, series), token))
                .Bind(_ => clientFactory.TableStorageNyaaCatalogWiper(request.SeriesId, token))
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

    // A series with a feed title and no binding is fed by SubsPlease.
    private static Task<Result<AnimeInfoStorage>> Bindable(
        string seriesId,
        SeriesSeason season,
        TvSeriesGetter seriesGetter,
        NyaaBindingGetter bindingGetter,
        CancellationToken token) =>
        seriesGetter(seriesId, season, token)
            .Bind(series => bindingGetter(seriesId, season, token)
                .Map(_ => true)
                .BindOnError(error => error is NotFoundError ? Result<bool>.Success(false) : error)
                .Bind(bound => bound || string.IsNullOrWhiteSpace(series.FeedTitle)
                    ? Result<AnimeInfoStorage>.Success(series)
                    : Error.Create($"{series.Title} is fed by SubsPlease and cannot be bound")));

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
