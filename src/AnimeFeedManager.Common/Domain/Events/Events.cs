namespace AnimeFeedManager.Common.Domain.Events;

public record DownloadImageEvent(
    string Partition,
    string Id,
    string Directory,
    string BlobName,
    string RemoteUrl,
    SeriesType SeriesType);

public record UpdateSeasonTitlesRequest(ImmutableList<string> Titles);

public record UpdateLatestSeasonsRequest(bool Update = true);

public record RemoveSubscriptionsRequest(string UserId);

public record CopySubscriptionRequest(string SourceId, string TargetId);

public record ScrapImagesRequest(ImmutableList<DownloadImageEvent> Events);

public record AddSeasonNotification(string Season, int Year, bool IsLatest);

public record MarkSeriesAsComplete(ImmutableList<string> Titles);

public record AutomatedSubscription(bool Value = true);

public record UpdateAlternativeTitle(string Id, string Season, string Title, string Original);

