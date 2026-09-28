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
        return JsonSerializer.Serialize(this, TvJsonContext.Default.ScrapTvLibraryResult);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent($"{Season.Year}-{Season.Season} library updated",
            builder =>
            {
                builder.OpenElement(1, "strong");
                builder.AddContent(2, NewSeries);
                builder.CloseElement();
                builder.AddContent(3, " new · ");
                builder.OpenElement(4, "strong");
                builder.AddContent(5, UpdatedSeries);
                builder.CloseElement();
                builder.AddContent(6, " updated");
            })
        {
            Actions = builder =>
            {
                builder.OpenElement(1, "a");
                builder.AddAttribute(2, "href", $"/{Season.Season}-{Season.Year}/tv");
                builder.AddAttribute(3, "class", "link text-xs font-semibold");
                builder.AddContent(4, "Go to season");
                builder.CloseElement();
            }
        };
    }
}
