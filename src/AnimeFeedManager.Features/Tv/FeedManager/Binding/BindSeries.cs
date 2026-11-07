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
            .SendOnError(domainPostman, [FailureEvent(request)], token)
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
        series.Status = SeriesStatus.Ongoing();
        return (series, bound);
    }

    // Schedule binds are covered by the run's summary toast: no toast or page refresh of their own.
    internal static DomainMessage[] Events(BindNyaaSeries request, BoundSeries bound) =>
    [
        new BackfillNyaaSeries(request.SeriesId),
        .. bound.WasOngoing ? Array.Empty<DomainMessage>() : [new UpdatedToOngoing(request.SeriesId, bound.FeedTitle)],
        .. request.Origin == BindingOrigin.Schedule
            ? Array.Empty<DomainMessage>()
            :
            [
                new SystemEvent(TargetConsumer.Admin(), EventTarget.Browser, EventType.Completed,
                    new NyaaSeriesBound(request.SeriesId, bound.Title, request.Group, request.Variant).AsEventPayload()),
                new AppEvent(TargetConsumer.Admin(), AppEventNames.NyaaBound, request.SeriesId)
            ]
    ];

    // Stored for the errors card either way; only a manual bind toasts.
    internal static SystemEvent FailureEvent(BindNyaaSeries request) =>
        new(TargetConsumer.Admin(),
            request.Origin == BindingOrigin.Schedule ? EventTarget.LocalStorage : EventTarget.Both,
            EventType.Error,
            new NyaaBindFailed(request.SeriesId, request.Group, request.ReleaseTitle).AsEventPayload());
}
