using AnimeFeedManager.Old.Common.Domain.Types;

namespace AnimeFeedManager.Old.Features.Notifications.Types;

public record FeedDetails(TorrentLink[] Links, string EpisodeInfo);
public record FeedInformation(Dictionary<string, FeedDetails[]> Feed);

public record EmptyFeed(): FeedInformation(new Dictionary<string, FeedDetails[]>());