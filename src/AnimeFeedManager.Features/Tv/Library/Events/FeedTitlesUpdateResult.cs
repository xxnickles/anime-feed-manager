namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record FeedTitlesUpdateResult(SeriesSeason Season, int UpdatedFeed) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, FeedTitlesUpdateResultContext.Default.FeedTitlesUpdateResult);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(FeedTitlesUpdateResult))]
[EventPayloadSerializerContext(typeof(FeedTitlesUpdateResult))]
public partial class FeedTitlesUpdateResultContext : JsonSerializerContext;

