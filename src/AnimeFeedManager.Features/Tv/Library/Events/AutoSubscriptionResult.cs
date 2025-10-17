namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record AutoSubscriptionResult(string SeriesId, int Count)
    : SystemNotificationPayload
{
    public override string AsJson()
    {
       return JsonSerializer.Serialize(this, AutoSubscriptionResultContext.Default.AutoSubscriptionResult);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(AutoSubscriptionResult))]
public partial class AutoSubscriptionResultContext : JsonSerializerContext;