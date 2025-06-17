using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public static class EventSending
{
    public static Task<Result<ScrapTvLibraryResult>> SendEvents(
        this Task<Result<ScrapTvLibraryData>> processData,
        IDomainPostman domainPostman,
        SeasonParameters? parameters,
        CancellationToken token) => processData
        .Map(data => (processData: data, Summary: ExtractResults(data)))
        .Bind(data => domainPostman.SendMessages(GetEvents(data), token)
            .Map(_ => data.Summary)
        ).MapError(e => domainPostman
            .SendMessage(
                new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Error,
                    GetErrorPayload(parameters)),
                token)
            .MatchToValue(_ => e, error => error)
        );


    private static EventPayload GetErrorPayload(SeasonParameters? parameters) =>
        new ScrapTvLibraryFailedResult(parameters is not null ? $"{parameters.Year}-{parameters.Season}" : "Latest")
            .AsEventPayload();

    private static DomainMessage[] GetEvents((ScrapTvLibraryData processData, ScrapTvLibraryResult summary) data) =>
    [
        new SeasonUpdated(data.processData.Season),
        new NewSeriesAdded(data.processData.SeriesData.Where(d => d.Image is AlreadyExistInSystem)
            .Select(d => d.Series.Title ?? string.Empty).ToArray()),
        new FeedUpdated(data.processData.FeedTitles.ToArray()),
        new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Completed,
            data.summary.AsEventPayload())
    ];

    private static ScrapTvLibraryResult ExtractResults(ScrapTvLibraryData data) =>
        new(data.Season,
            data.SeriesData.Count(d => d.Image is AlreadyExistInSystem),
            data.SeriesData.Count(d => d.Image is not AlreadyExistInSystem));
}