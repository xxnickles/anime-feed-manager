namespace AnimeFeedManager.Features.Feeds.Entities;

/// <summary>
/// One user's subscription to a series — colocated with that series' classification, confirmation
/// markers, and release history so the notification-dispatch job finds subscribers in the same
/// partition query it already runs. Subscribe/unsubscribe are plain create/delete on this document;
/// no shared document to merge, so concurrent subscribers never contend. The forward side (which
/// series a given user subscribes to) lives in the <c>users</c> container as <c>UserSubscriptions</c>.
/// <see cref="LastNotifiedAt"/> is null until the dispatch job's first successful send to this
/// subscriber; a release with <see cref="ReleaseDetected.DetectedAt"/> after this marker (or a null
/// marker) is "unseen" by them — this is what lets a late subscriber still get caught up without
/// re-notifying everyone else, and lets a failed send retry on the next pass instead of advancing.
/// </summary>
public sealed record SeriesSubscriber : SeriesFeedsDocument
{
    public string UserId { get; }
    public DateTimeOffset? LastNotifiedAt { get; init; }

    public SeriesSubscriber(int seriesId, string userId) : base(seriesId)
    {
        UserId = userId;
        Id = $"subscriber:{seriesId}:{userId}";
    }
}
