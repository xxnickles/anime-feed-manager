namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record ScrapTvLibraryFailedResult(string Season, UpdateType UpdateType = UpdateType.FullLibrary)
    : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, ScrapTvLibraryFailedResultContext.Default.ScrapTvLibraryFailedResult);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent(
            $"TV library for {Season} has been failed",
            builder =>
            {
                // Body
                builder.AddContent(1, "Scraping process has been failed.");
            });
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(ScrapTvLibraryFailedResult))]
[EventPayloadSerializerContext(typeof(ScrapTvLibraryFailedResult))]
public partial class ScrapTvLibraryFailedResultContext : JsonSerializerContext;