namespace AnimeFeedManager.Features.Seasons.Events;

public sealed record SeasonUpdated(SeriesSeason Season) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "season-updated-events";
}