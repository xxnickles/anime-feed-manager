namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record FeedTitlesUpdateError(SeriesSeason Season) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.FeedTitlesUpdateError);
    }
}
