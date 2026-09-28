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
                NewSeason => new SeasonUpdateResult(seriesSeason, SeasonUpdateStatus.New),
                _ => new SeasonUpdateResult(seriesSeason, SeasonUpdateStatus.Updated)
            })
            .Bind(r => domainPostman(GetOnCompletedEvents(r), cancellationToken)
                .Map(_ => r))
            .MapError(e => domainPostman([GetOnErrorEvent(seriesSeason)], cancellationToken)
                .MatchToValue(_ => e, error => error));


    // Only a real creation reaches the browser; every outcome is kept in the admin event history.
    private static SystemEvent[] GetOnCompletedEvents(SeasonUpdateResult data) =>
        data.SeasonUpdateStatus is SeasonUpdateStatus.New
            ? [CreateStoredEvent(data, EventType.Completed), CreateNewSeasonEvent(data.Season)]
            : [CreateStoredEvent(data, EventType.Completed)];

    private static SystemEvent GetOnErrorEvent(SeriesSeason data) =>
        CreateStoredEvent(new SeasonUpdateResult(data, SeasonUpdateStatus.Error), EventType.Error);

    private static SystemEvent CreateStoredEvent(SeasonUpdateResult data, EventType type) => new(
        TargetConsumer.Admin(), EventTarget.LocalStorage, type, data.AsEventPayload());

    private static SystemEvent CreateNewSeasonEvent(SeriesSeason season) => new(
        TargetConsumer.Everybody(), EventTarget.Browser, EventType.Information,
        new NewSeasonAdded(season).AsEventPayload());
}
