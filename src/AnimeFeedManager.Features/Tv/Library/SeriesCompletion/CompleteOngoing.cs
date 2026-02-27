using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.SeriesCompletion;

public static class CompleteOngoing
{
    public static Task<Result<CompleteOnGoingTvSeriesProcessResult>> CompleteOngoingSeries(
        string[] updatedFeed,
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        CancellationToken token)
    {
        return StartProcess(clientFactory.TableStorageOnGoingStoredTvSeries, updatedFeed, token)
            .StoreChanges(clientFactory.TableStorageTvLibraryUpdater, token)
            .SendEvents(domainPostman, token)
            .Map(r => new CompleteOnGoingTvSeriesProcessResult(r.SeriesToComplete));
    }

    internal static Task<Result<CompleteOnGoingTvSeriesProcess>> StartProcess(OnGoingStoredTvSeries seriesGetter,
        string[] updatedFeed,
        CancellationToken token)
    {
        return seriesGetter(token)
            .WithOperationName(nameof(StartProcess))
            .Map(ongoingSeries => ongoingSeries
                .Where(s => !updatedFeed.Contains(s.FeedTitle))
                .Select(Complete))
            .Map(series => new CompleteOnGoingTvSeriesProcess(series.ToImmutableArray()));
    }

    extension(Task<Result<CompleteOnGoingTvSeriesProcess>> process)
    {
        internal Task<Result<CompleteOnGoingTvSeriesProcess>> StoreChanges(TvLibraryStorageUpdater updater,
            CancellationToken token)
        {
            return process.Bind(p => updater(p.SeriesToComplete, token)
                .WithOperationName(nameof(StoreChanges))
                .WithLogProperty("Series", p.SeriesToComplete)
                .Map(_ => p)
            );
        }

        private Task<Result<CompleteOnGoingTvSeriesProcess>> SendEvents(DomainCollectionSender domainPostman,
            CancellationToken token) => process
            .BindWhen(
                data => domainPostman([GetEvent(data)], token).Map(_ => data),
                data => data.SeriesToComplete.Length > 0)
            .MapError(e => domainPostman(
                    [
                        new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Error,
                            new CompletedTvSeriesResult([], ResultType.Failed).AsEventPayload())
                    ],
                    token)
                .MatchToValue(_ => e, error => error));
    }


    private static SystemEvent GetEvent(CompleteOnGoingTvSeriesProcess data) =>
        new(
            TargetConsumer.Admin(),
            EventTarget.Both,
            EventType.Completed,
            new CompletedTvSeriesResult(data.SeriesToComplete.Select(d => d.Title ?? string.Empty).ToArray(),
                ResultType.Success).AsEventPayload());

    private static AnimeInfoStorage Complete(AnimeInfoStorage series)
    {
        series.Status = SeriesStatus.Completed();
        return series;
    }
}