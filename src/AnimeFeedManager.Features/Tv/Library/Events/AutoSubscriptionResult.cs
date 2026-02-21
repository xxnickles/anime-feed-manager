namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record AutoSubscriptionResult(string SeriesId, int Count)
    : SystemNotificationPayload
{
    public override string AsJson()
    {
       return JsonSerializer.Serialize(this, TvJsonContext.Default.AutoSubscriptionResult);
    }
}
