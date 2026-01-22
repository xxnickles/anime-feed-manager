using AnimeFeedManager.Features.Seasons.Events;

namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

public static class EventSending
{
    public static Task<Result<SeasonUpdateResult>> SentEvents(
        this Task<Result<SeasonUpdateData>> result,
        DomainCollectionSender domainPostman,
        SeriesSeason seriesSeason,
        CancellationToken cancellationToken) =>
        result
            .Map(d => d.SeasonData switch
            {
                NoUpdateRequired or NoMatch => new SeasonUpdateResult(seriesSeason, SeasonUpdateStatus.NoChanges),
                NewSeason or ReplaceLatestSeason => new SeasonUpdateResult(seriesSeason, SeasonUpdateStatus.New),
                _ => new SeasonUpdateResult(seriesSeason, SeasonUpdateStatus.Updated)
            })
            .Bind(r => domainPostman([GetOnCompletedEvent(r)], cancellationToken)
                .Map(_ => r))
            .MapError(e => domainPostman([GetOnErrorEvent(seriesSeason)], cancellationToken)
                .MatchToValue(_ => e, error => error));


    private static SystemEvent GetOnCompletedEvent(SeasonUpdateResult data) => CreateEvent(data, EventType.Completed);

    private static SystemEvent GetOnErrorEvent(SeriesSeason data) =>
        CreateEvent(new SeasonUpdateResult(data, SeasonUpdateStatus.Error), EventType.Error);


    private static SystemEvent CreateEvent(SeasonUpdateResult data, EventType type) => new(TargetConsumer.Admin(),
        EventTarget.Both, type, data.AsEventPayload());
}