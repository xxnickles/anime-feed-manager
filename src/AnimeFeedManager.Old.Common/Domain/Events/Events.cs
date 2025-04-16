namespace AnimeFeedManager.Common.Domain.Events;

public record DownloadImageEvent(
    string Partition,
    string Id,
    string Directory,
    string BlobName,
    string RemoteUrl,
    SeriesType SeriesType)
    : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "image-process";
}

public record UpdateSeasonTitlesRequest(ImmutableList<string> Titles) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "season-titles-process";
}

public record UpdateLatestSeasonsRequest(bool Update = true) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "latest-seasons-process";
}

public record RemoveSubscriptionsRequest(string UserId) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "subscriptions-removal";
}

public record CopySubscriptionRequest(string SourceId, string TargetId) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "subscriptions-copy";
}

public record ScrapImagesRequest(ImmutableList<DownloadImageEvent> Events) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "image-to-scrap";
}

public record AddSeasonNotification(string Season, int Year, bool IsLatest) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "add-seasson";
}

public record MarkSeriesAsComplete(ImmutableList<string> Titles) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "series-completer";
}

public record AutomatedSubscription(bool Value = true) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "automated-subscription";
}

public record UpdateAlternativeTitle(string Id, string Season, string Title, string Original, string Status): DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "alternative-title-update-box";
}