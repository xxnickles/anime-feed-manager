namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record FeedTitlesUpdateResult(SeriesSeason Season, int UpdatedFeed) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.FeedTitlesUpdateResult);
    }
}

