using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Features.Tv.Library.Events;

public enum UpdateType
{
    FullLibrary,
    Titles
}

public enum ResultType
{
    Success,
    Failed
}

public sealed record ScrapTvLibraryResult(
    SeriesSeason Season,
    int UpdatedSeries,
    int NewSeries,
    UpdateType UpdateType = UpdateType.FullLibrary)
    : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, ScrapTvLibraryResultContext.Default.ScrapTvLibraryResult);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent ($"TV library for {Season.Year}-{Season.Season} has been scrapped successfully",
            builder =>
            {
                // First strong element
                builder.OpenElement(1, "strong");
                builder.AddContent(2, NewSeries);
                builder.CloseElement(); // Close strong
                // Text between strong elements
                builder.AddContent(3, " new series has been added. ");
                // Second strong element
                builder.OpenElement(4, "strong");
                builder.AddContent(5, UpdatedSeries);
                builder.CloseElement(); // Close strong
                // Final text
                builder.AddContent(6, " has been updated.");
            });
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(ScrapTvLibraryResult))]
[EventPayloadSerializerContext(typeof(ScrapTvLibraryResult))]
public partial class ScrapTvLibraryResultContext : JsonSerializerContext;