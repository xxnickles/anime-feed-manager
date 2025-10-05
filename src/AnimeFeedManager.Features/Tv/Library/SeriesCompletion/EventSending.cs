using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.SeriesCompletion;

internal static class EventSending
{
    internal static Task<Result<CompleteOnGoingTvSeriesProcess>> SendEvents(
        this Task<Result<CompleteOnGoingTvSeriesProcess>> processData,
        IDomainPostman domainPostman,
        CancellationToken token) => processData
        .Bind(data => domainPostman.SendMessages(GetEvents(data), token)
            .Map(_ => data))
        .MapError(e => domainPostman
            .SendMessage(
                new SystemEvent(TargetConsumer.Everybody(), EventTarget.Both, EventType.Error,
                    new CompletedTvSeriesResult([], ResultType.Failed).AsEventPayload()),
                token)
            .MatchToValue(_ => e, error => error));


    private static DomainMessage[] GetEvents(CompleteOnGoingTvSeriesProcess data) =>
    [
        new SystemEvent(
            TargetConsumer.Admin(),
            EventTarget.Both,
            EventType.Completed,
            new CompletedTvSeriesResult(data.SeriesToComplete.Select(d => d.Title ?? string.Empty).ToArray(),
                ResultType.Success).AsEventPayload())
    ];
}