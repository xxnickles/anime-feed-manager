using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public static class EventSending
{
    public static Task<Result<ScrapTvLibraryResult, DomainError>> SendEvents(
        this Task<Result<ScrapTvLibraryData, DomainError>> processData,
        DomainCollectionSender domainPostman,
        SeasonParameters? parameters,
        CancellationToken token) => processData
        .Map(data => (processData: data, Summary: ExtractResults(data)))
        .Bind(data => domainPostman(GetEvents(data), token)
            .Map(_ => data.Summary)
        ).SendOnError(domainPostman,
            [
                new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Error,
                    GetErrorPayload(parameters))
            ],
            token);


    private static EventPayload GetErrorPayload(SeasonParameters? parameters) =>
        new ScrapTvLibraryFailedResult(parameters is not null ? $"{parameters.Year}-{parameters.Season}" : "Current")
            .AsEventPayload();

    private static DomainMessage[] GetEvents((ScrapTvLibraryData processData, ScrapTvLibraryResult summary) data) =>
    [
        new SeasonUpdated(data.processData.Season),
        new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Completed,
            data.summary.AsEventPayload())
    ];

    private static ScrapTvLibraryResult ExtractResults(ScrapTvLibraryData data) =>
        new(data.Season,
            data.SeriesData.Count(d => d.Status is Status.UpdatedSeries),
            data.SeriesData.Count(d => d.Status is Status.NewSeries));
}