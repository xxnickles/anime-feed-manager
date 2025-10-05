namespace AnimeFeedManager.Features.Tv.Library.SeriesCompletion;

public static class CompleteOngoing
{
    public static Task<Result<CompleteOnGoingTvSeriesProcessResult>> CompleteOngoingSeries(
        ITableClientFactory clientFactory,
        IDomainPostman domainPostman,
        string[] updatedFeed,
        CancellationToken token)
    {
        return StartProcess(clientFactory.TableStorageOnGoingStoredTvSeries(), updatedFeed, token)
            .StoreChanges(clientFactory.TableStorageTvLibraryUpdater(), token)
            .SendEvents(domainPostman, token)
            .Map(r => new CompleteOnGoingTvSeriesProcessResult(r.SeriesToComplete));
    }

    private static Task<Result<CompleteOnGoingTvSeriesProcess>> StartProcess(OnGoingStoredTvSeries seriesGetter,
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

    private static Task<Result<CompleteOnGoingTvSeriesProcess>> StoreChanges(
        this Task<Result<CompleteOnGoingTvSeriesProcess>> process, TvLibraryStorageUpdater updater,
        CancellationToken token)
    {
        return process.Bind(p => updater(p.SeriesToComplete, token)
            .Map(_ => p)
            .MapError(e => e.WithOperationName(nameof(StoreChanges))
                .WithLogProperty("Series", p.SeriesToComplete))
        );
    }

    private static AnimeInfoStorage Complete(AnimeInfoStorage series)
    {
        series.Status = SeriesStatus.Completed();
        return series;
    }
}