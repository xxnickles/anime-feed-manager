using AnimeFeedManager.Features.Feeds.Entities;

namespace AnimeFeedManager.Features.Notifications;

/// <summary>
/// One subscriber's unseen releases for a single series — releases with <see cref="ReleaseDetected.DetectedAt"/>
/// after <see cref="SeriesSubscriber.LastNotifiedAt"/>, or all of them when the marker is null. Only
/// produced when at least one release qualifies; an already-caught-up subscriber yields nothing.
/// </summary>
public sealed record UnseenRelease(SeriesSubscriber Subscriber, ImmutableArray<ReleaseDetected> Releases);

/// <summary>One user's unseen releases across every series scanned in a dispatch pass — a digest email's content.</summary>
public sealed record UserDigest(string UserId, ImmutableArray<UnseenRelease> Series);

/// <summary>
/// Matches subscribers against releases for one series (single-partition data, already loaded), then
/// aggregates matches from multiple series into one digest per user. <see cref="ReleaseDetected.Confirmed"/>
/// and <see cref="ReleaseDetectedStatus"/> never gate a match — only <see cref="SeriesSubscriber.LastNotifiedAt"/>
/// does, which is what lets an already-<see cref="ReleaseDetectedStatus.Processed"/> release still reach a
/// newly-subscribed user.
/// </summary>
public static class NotificationMatching
{
    public static ImmutableArray<UnseenRelease> MatchSeries(
        ImmutableArray<ReleaseDetected> releases, ImmutableArray<SeriesSubscriber> subscribers) =>
        [.. subscribers
            .Select(subscriber => new UnseenRelease(subscriber, Unseen(releases, subscriber.LastNotifiedAt)))
            .Where(match => match.Releases.Length > 0)];

    /// <summary>Groups matches from every series scanned this pass by <see cref="SeriesSubscriber.UserId"/>.</summary>
    public static ImmutableArray<UserDigest> AggregateByUser(IEnumerable<UnseenRelease> matches) =>
        [.. matches
            .GroupBy(match => match.Subscriber.UserId)
            .Select(group => new UserDigest(group.Key, [.. group]))];

    /// <summary>The <see cref="SeriesSubscriber.LastNotifiedAt"/> value to persist once this match's digest send succeeds.</summary>
    public static DateTimeOffset NewMarker(this UnseenRelease match) =>
        match.Releases.Max(release => release.DetectedAt);

    private static ImmutableArray<ReleaseDetected> Unseen(
        ImmutableArray<ReleaseDetected> releases, DateTimeOffset? lastNotifiedAt) =>
        [.. releases.Where(release => lastNotifiedAt is null || release.DetectedAt > lastNotifiedAt)];
}
