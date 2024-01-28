﻿namespace AnimeFeedManager.Common.Domain.Events;

public record DownloadImageEvent(
    string Partition,
    string Id,
    string Directory,
    string BlobName,
    string RemoteUrl,
    SeriesType SeriesType);

public record UpdateSeasonTitlesRequest(ImmutableList<string> Titles);

public record UpdateLatestSeasonsRequest(bool Update = true);

public record RemoveSubscriptionsRequest(string userId);

public record CopySubscriptionRequest(string sourceId, string targetId);