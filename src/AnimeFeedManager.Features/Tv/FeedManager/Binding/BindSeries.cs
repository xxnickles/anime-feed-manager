using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Binding;

public sealed record BoundSeries(string Title, string FeedTitle, bool WasOngoing);

public static class BindSeries
{
    /// <summary>Replaces the binding, feeds the library series from it, then queues the backfill; the catalog is kept.</summary>
    public static Task<Result<BoundSeries>> Bind(
        BindNyaaSeries request,
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        CancellationToken token) =>
        IdHelpers.SeriesSeasonFromId(request.SeriesId)
            .Bind(season => clientFactory.TableStorageTvSeriesGetter(request.SeriesId, season, token)
                .Bind(series => clientFactory.TableStorageNyaaBindingReplacer(NewBinding(request, season, series), token))
                .Bind(_ => FeedLibrarySeries(request, season,
                    clientFactory.TableStorageTvSeriesGetter, clientFactory.TableStorageTvSeriesUpdater, token)))
            .Bind(bound => domainPostman(Events(request, bound), token).Map(_ => bound))
            .SendOnError(domainPostman,
                [
                    new SystemEvent(TargetConsumer.Admin(), EventTarget.Both, EventType.Error,
                        new NyaaBindFailed(request.SeriesId, request.Group, request.ReleaseTitle, request.RunId)
                            .AsEventPayload())
                ],
                token)
            .WithOperationName(nameof(Bind))
            .WithLogProperty(Telemetry.SeriesIdTag, request.SeriesId);

    /// <summary>An existing feed title is kept: it is the subscription key across seasons and rebinds.</summary>
    internal static string FeedTitle(AnimeInfoStorage series, BindNyaaSeries request) =>
        !string.IsNullOrWhiteSpace(series.FeedTitle) ? series.FeedTitle
        : request.Origin == BindingOrigin.Schedule ? request.ReleaseTitle
        : series.Title ?? string.Empty;

    private static NyaaBindingStorage NewBinding(BindNyaaSeries request, SeriesSeason season, AnimeInfoStorage series) =>
        new()
        {
            PartitionKey = IdHelpers.GenerateAnimePartitionKey(season),
            RowKey = request.SeriesId,
            Group = request.Group,
            ReleaseTitle = request.ReleaseTitle,
            Variant = request.Variant,
            FeedTitle = FeedTitle(series, request),
            Trusted = request.Trusted,
            Tracking = true,
            Episodes = series.Episodes,
            Origin = request.Origin,
            Premiere = series.Date is { } date ? new DateTimeOffset(DateTime.SpecifyKind(date, DateTimeKind.Utc)) : null
        };

    private static Task<Result<BoundSeries>> FeedLibrarySeries(
        BindNyaaSeries request,
        SeriesSeason season,
        TvSeriesGetter seriesGetter,
        TvSeriesStorageUpdater seriesUpdater,
        CancellationToken token) =>
        OptimisticConcurrency.UntilWritten(
            attemptToken => seriesGetter(request.SeriesId, season, attemptToken)
                .Map(series => FeedFromBinding(series, request))
                .Bind(fed => seriesUpdater(fed.Series, attemptToken).Map(_ => fed.Bound)),
            token,
            error => error is PreconditionFailedError);

    private static (AnimeInfoStorage Series, BoundSeries Bound) FeedFromBinding(AnimeInfoStorage series,
        BindNyaaSeries request)
    {
        var bound = new BoundSeries(series.Title ?? string.Empty, FeedTitle(series, request),
            series.Status == SeriesStatus.OngoingValue);
        series.FeedTitle = bound.FeedTitle;
        series.FeedLink = FeedManagerRoutes.Releases(request.SeriesId);
        series.FeedSource = SeriesFeeds.LocalSource;
        series.Status = SeriesStatus.Ongoing();
        return (series, bound);
    }

    // Schedule binds are announced by the feed job summary: stored for stats, no toast or page refresh.
    private static DomainMessage[] Events(BindNyaaSeries request, BoundSeries bound)
    {
        var automatic = request.Origin == BindingOrigin.Schedule;
        return
        [
            new BackfillNyaaSeries(request.SeriesId, Quiet: automatic),
            .. bound.WasOngoing ? Array.Empty<DomainMessage>() : [new UpdatedToOngoing(request.SeriesId, bound.FeedTitle)],
            new SystemEvent(TargetConsumer.Admin(), automatic ? EventTarget.LocalStorage : EventTarget.Both,
                EventType.Completed,
                new NyaaSeriesBound(request.SeriesId, bound.Title, request.Group, request.Variant, request.Origin,
                        request.RunId)
                    .AsEventPayload()),
            .. automatic ? Array.Empty<DomainMessage>() : [new AppEvent(TargetConsumer.Admin(), AppEventNames.NyaaBound, request.SeriesId)]
        ];
    }
}
