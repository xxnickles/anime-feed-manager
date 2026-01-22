using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.TitlesScrapProcess;

public static class EventSending
{
    public static Task<Result<ScrapTvLibraryResult>> SendEvents(
        this Task<Result<FeedTitleUpdateData>> processData,
        DomainCollectionSender domainPostman,
        CancellationToken token) => processData
        .Map(data => (processData: data, Summary: ExtractResults(data)))
        .Bind(data => domainPostman(GetEvents(data), token)
            .Map(_ => data.Summary)
        ).MapError(e => domainPostman(
                [
                    new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Error,
                        GetErrorPayload())
                ],
                token)
            .MatchToValue(_ => e, error => error)
        );


    private static ScrapTvLibraryResult ExtractResults(FeedTitleUpdateData data) =>
        new(data.Season,
            data.FeedTitleUpdateInformation.Count(d => d.UpdateStatus == UpdateStatus.Updated),
            0, UpdateType.Titles);


    private static DomainMessage[] GetEvents((FeedTitleUpdateData processData, ScrapTvLibraryResult summary) data)
    {
        var titles = data.processData.FeedData.Select(f => f.Title).ToArray();
        return
        [
            new FeedTitlesUpdated(data.processData.Season, titles),
            new CompleteOngoingSeries(titles),
            new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Completed,
                data.summary.AsEventPayload()),
            .. GetUpdatedToOngoingEvents(data.processData),
            .. GetFeedUpdatedEvents(data.processData)
        ];
    }


    private static UpdatedToOngoing[] GetUpdatedToOngoingEvents(FeedTitleUpdateData data)
    {
        return data.FeedTitleUpdateInformation.Where(i => i.UpdateStatus == UpdateStatus.Updated)
            .Select(i => new UpdatedToOngoing(i.Series.RowKey ?? string.Empty, i.Series.FeedTitle ?? string.Empty))
            .ToArray();
    }

    private static DomainMessage[] GetFeedUpdatedEvents(FeedTitleUpdateData data)
    {
        return data.FeedTitleUpdateInformation.Where(d => d.UpdateStatus == UpdateStatus.Updated)
            .Select(d => new SeriesFeedUpdated(d.Series.RowKey ?? string.Empty, d.Series.FeedTitle ?? string.Empty))
            .ToArray<DomainMessage>();
    }

    private static EventPayload GetErrorPayload() =>
        new ScrapTvLibraryFailedResult("Titles", UpdateType.Titles)
            .AsEventPayload();
}