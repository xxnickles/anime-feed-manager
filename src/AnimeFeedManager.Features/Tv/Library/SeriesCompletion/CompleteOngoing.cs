using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.SeriesCompletion;

public static class CompleteOngoing
{
    public static Task<Result<CompleteOnGoingTvSeriesProcessResult>> CompleteOngoingSeries(
        string[] updatedFeed,
        ITableClientFactory clientFactory,
        IDomainPostman domainPostman,
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
            .Map(ongoingSeries => ongoingSeries
                .Where(s => !updatedFeed.Contains(s.FeedTitle))
                .Select(Complete))
            .Map(series => new CompleteOnGoingTvSeriesProcess(series.ToImmutableList()))
            .MapError(error => error.WithOperationName(nameof(StartProcess)));
    }

    extension(Task<Result<CompleteOnGoingTvSeriesProcess>> process)
    {
        internal Task<Result<CompleteOnGoingTvSeriesProcess>> StoreChanges(TvLibraryStorageUpdater updater,
            CancellationToken token)
        {
            return process.Bind(p => updater(p.SeriesToComplete, token)
                .Map(_ => p)
                .MapError(e => e.WithOperationName(nameof(StoreChanges))
                    .WithLogProperty("Series", p.SeriesToComplete))
            );
        }

        internal Task<Result<CompleteOnGoingTvSeriesProcess>> SendEvents(IDomainPostman domainPostman,
            CancellationToken token) => process
            .Bind(data => domainPostman.SendMessage(GetEvent(data), token)
                .Map(_ => data))
            .MapError(e => domainPostman
                .SendMessage(
                    new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Error,
                        new CompletedTvSeriesResult([], ResultType.Failed).AsEventPayload()),
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