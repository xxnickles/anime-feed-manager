namespace AnimeFeedManager.Features.Feeds.Entities;

/// <summary>
/// One user's subscription to a series — colocated with that series' classification, confirmation
/// markers, and release history so the notification-dispatch job finds subscribers in the same
/// partition query it already runs. Subscribe/unsubscribe are plain create/delete on this document;
/// no shared document to merge, so concurrent subscribers never contend. The forward side (which
/// series a given user subscribes to) lives in the <c>users</c> container as <c>UserSubscriptions</c>.
/// </summary>
public sealed record SeriesSubscriber : SeriesFeedsDocument
{
    public string UserId { get; }

    public SeriesSubscriber(int seriesId, string userId) : base(seriesId)
    {
        UserId = userId;
        Id = $"subscriber:{seriesId}:{userId}";
    }
}
