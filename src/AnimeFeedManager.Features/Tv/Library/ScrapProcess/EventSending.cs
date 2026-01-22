using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public static class EventSending
{
    public static Task<Result<ScrapTvLibraryResult>> SendEvents(
        this Task<Result<ScrapTvLibraryData>> processData,
        DomainCollectionSender domainPostman,
        SeasonParameters? parameters,
        CancellationToken token) => processData
        .Map(data => (processData: data, Summary: ExtractResults(data)))
        .Bind(data => domainPostman(GetEvents(data), token)
            .Map(_ => data.Summary)
        ).MapError(e => domainPostman(
                [
                    new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Error,
                        GetErrorPayload(parameters))
                ],
                token)
            .MatchToValue(_ => e, error => error)
        );


    private static EventPayload GetErrorPayload(SeasonParameters? parameters) =>
        new ScrapTvLibraryFailedResult(parameters is not null ? $"{parameters.Year}-{parameters.Season}" : "Latest")
            .AsEventPayload();

    private static DomainMessage[] GetEvents((ScrapTvLibraryData processData, ScrapTvLibraryResult summary) data)
    {
        var titles = data.processData.FeedData.Select(f => f.Title).ToArray();
        return
        [
            new SeasonUpdated(data.processData.Season),
            new FeedTitlesUpdated(data.processData.Season, titles),
            new CompleteOngoingSeries(titles),
            new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Completed,
                data.summary.AsEventPayload()),
            .. GetCompletedSeriesEvent(data.processData),
            .. GetUpdatedToOngoingEvents(data.processData),
            .. GetFeedUpdatedEvents(data.processData)
        ];
    }


    private static CompletedSeries[] GetCompletedSeriesEvent(ScrapTvLibraryData data)
    {
        return data.SeriesData.Where(s => s.Series.Status == SeriesStatus.Completed())
            .Select(s => new CompletedSeries(s.Series.RowKey ?? string.Empty))
            .ToArray();
    }

    private static DomainMessage[] GetFeedUpdatedEvents(ScrapTvLibraryData data)
    {
        return data.SeriesData.Where(d => !string.IsNullOrEmpty(d.Series.FeedTitle))
            .Select(d => new SeriesFeedUpdated(d.Series.RowKey ?? string.Empty, d.Series.FeedTitle ?? string.Empty))
            .ToArray<DomainMessage>();
    }

    private static ScrapTvLibraryResult ExtractResults(ScrapTvLibraryData data) =>
        new(data.Season,
            data.SeriesData.Count(d => d.Status is Status.UpdatedSeries),
            data.SeriesData.Count(d => d.Status is Status.NewSeries));

    private static UpdatedToOngoing[] GetUpdatedToOngoingEvents(ScrapTvLibraryData data) => data.SeriesData.Where(d =>
            d.Status is not Status.NoChanges && d.Series.Status == SeriesStatus.Ongoing())
        .Select(d => new UpdatedToOngoing(d.Series.RowKey ?? string.Empty, d.Series.FeedTitle ?? string.Empty))
        .ToArray();
}