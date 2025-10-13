namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record FeedTitlesUpdateError(SeriesSeason Season) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, FeedTitlesUpdateErrorContext.Default.FeedTitlesUpdateError);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(FeedTitlesUpdateError))]
[EventPayloadSerializerContext(typeof(FeedTitlesUpdateError))]
public partial class FeedTitlesUpdateErrorContext : JsonSerializerContext;