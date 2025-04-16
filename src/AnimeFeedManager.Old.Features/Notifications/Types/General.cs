using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Features.Notifications.Types;

public record FeedDetails(TorrentLink[] Links, string EpisodeInfo);
public record FeedInformation(Dictionary<string, FeedDetails[]> Feed);

public record EmptyFeed(): FeedInformation(new Dictionary<string, FeedDetails[]>());