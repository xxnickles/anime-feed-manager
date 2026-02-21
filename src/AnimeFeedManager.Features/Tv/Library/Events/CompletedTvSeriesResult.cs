namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record CompletedTvSeriesResult(string[] Titles, ResultType ResultType)
    : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.CompletedTvSeriesResult);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent(GetTitle(),
            builder =>
            {
                // Body
                builder.AddContent(0, GetBody());
            });
    }

    private string GetBody()
    {
        return ResultType switch
        {
            ResultType.Failed => "Completion process for TV Series has failed after feed updates.",
            ResultType.Success => $"{Titles.Length} series have been completed successfully.",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string GetTitle()
    {
        return ResultType switch
        {
            ResultType.Failed => "Completing TV Series process has failed.",
            ResultType.Success => "Completing TV Series process has been completed successfully.",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
