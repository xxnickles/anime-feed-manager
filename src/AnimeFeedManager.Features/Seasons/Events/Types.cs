namespace AnimeFeedManager.Features.Seasons.Events;

public enum SeasonUpdateStatus
{
    New,
    Updated,
    NoChanges,
    Error
}


public sealed record SeasonUpdateResult(SeriesSeason Season, SeasonUpdateStatus SeasonUpdateStatus)
    : SystemNotificationPayload
{
    public override string AsJson()
    {
       return JsonSerializer.Serialize(this, SeasonUpdatedResultContext.Default.SeasonUpdateResult);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent(GetNotificationTitle(),
            builder =>
            {
                // First strong element
                builder.OpenElement(1, "strong");
                builder.AddContent(2, $"{Season.Year}-{Season.Season}");
                builder.CloseElement(); // Close strong
                // Text between strong elements
                builder.AddContent(3, GetNotificationBody(SeasonUpdateStatus));
            });
    }
    
    private string GetNotificationTitle() =>
        SeasonUpdateStatus switch
        {
            SeasonUpdateStatus.New =>
                $"Season {Season.Year}-{Season.Season} has been created successfully",
            SeasonUpdateStatus.Updated =>
                $"Season {Season.Year}-{Season.Season} has been updated successfully",
            SeasonUpdateStatus.NoChanges =>
                $"Season {Season.Year}-{Season.Season} has been processed successfully",
            SeasonUpdateStatus.Error =>
                $"Season {Season.Year}-{Season.Season} process has failed",
            _ => throw new ArgumentOutOfRangeException(nameof(SeasonUpdateStatus),
                SeasonUpdateStatus, $"Unknown '{SeasonUpdateStatus}' status")
        };

    private static string GetNotificationBody(SeasonUpdateStatus status) =>
        status switch
        {
            SeasonUpdateStatus.New => " has beed added to the system",
            SeasonUpdateStatus.Updated => " was already in the system and has been updated ",
            SeasonUpdateStatus.NoChanges => " was already in the system; no changes were made",
            SeasonUpdateStatus.Error => " update has failed",
            _ => throw new ArgumentOutOfRangeException(nameof(SeasonUpdateStatus),
                status, $"Unknown '{status}' status")
        };
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(SeasonUpdateResult))]
[EventPayloadSerializerContext(typeof(SeasonUpdateResult))]
public partial class SeasonUpdatedResultContext : JsonSerializerContext;
