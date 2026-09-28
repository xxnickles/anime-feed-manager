namespace AnimeFeedManager.Features.Seasons.Events;

public enum SeasonUpdateStatus
{
    New,
    Updated,
    NoChanges,
    Error
}


/// <summary>A season created in the system. Sticky, with a page-refresh action so open clients pick it up.</summary>
public sealed record NewSeasonAdded(SeriesSeason Season) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, SeasonsJsonContext.Default.NewSeasonAdded);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent("New season available",
            builder =>
            {
                builder.OpenElement(1, "strong");
                builder.AddContent(2, $"{Season.Year}-{Season.Season}");
                builder.CloseElement();
                builder.AddContent(3, " has been added. Refresh the page to see it.");
            })
        {
            Actions = builder =>
            {
                builder.OpenElement(1, "button");
                builder.AddAttribute(2, "type", "button");
                builder.AddAttribute(3, "class", "btn btn-sm btn-primary");
                builder.AddAttribute(4, "_", "on click call location.reload()");
                builder.AddContent(5, "Refresh");
                builder.CloseElement();
            },
            Lifetime = new Sticky()
        };
    }
}

public sealed record SeasonUpdateResult(SeriesSeason Season, SeasonUpdateStatus SeasonUpdateStatus)
    : SystemNotificationPayload
{
    public override string AsJson()
    {
       return JsonSerializer.Serialize(this, SeasonsJsonContext.Default.SeasonUpdateResult);
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
