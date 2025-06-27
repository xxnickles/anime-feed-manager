using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.FeedTitlesUpdateProcess;

public static class EventSending
{
    public static Task<Result<FeedTitlesUpdateResult>> SentEvents(
        this Task<Result<FeedTitlesUpdateResult>> processData,
        IDomainPostman domainPostman,
        SeriesSeason defaultSeriesSeason,
        CancellationToken token) => processData
        .Bind(data => domainPostman.SendMessages(GetEvents(data), token).Map(_ => data))
        .MapError(e => domainPostman
            .SendMessage(
                new SystemEvent(TargetConsumer.Admin(), EventTarget.LocalStorage, EventType.Error,
                    GetErrorPayload(defaultSeriesSeason)),
                token)
            .MatchToValue(_ => e, error => error)
        );


    private static DomainMessage[] GetEvents(FeedTitlesUpdateResult processResult) =>
    [
        new SystemEvent(TargetConsumer.Admin(), EventTarget.LocalStorage, EventType.Completed,
            processResult.AsEventPayload())
    ];

    private static EventPayload GetErrorPayload(SeriesSeason season) =>
        new FeedTitlesUpdateError(season).AsEventPayload();
}