namespace AnimeFeedManager.Features.Subscriptions.Entities;

/// <summary>
/// One user's subscription to a series (users container) — the forward side, mirroring
/// <c>SeriesSubscriber</c>'s reverse side in the feeds container. Colocated with the user's
/// account in the same partition. Plain create/delete; no shared document, so a user's own
/// concurrent subscribe/unsubscribe actions never contend. <see cref="Season"/> is
/// <c>Library.Series</c>'s own partition key value, carried here so a later point-read against
/// the series container never needs a cross-partition query.
/// </summary>
public sealed record UserSubscription : UserDocument
{
    public int SeriesId { get; }

    public SeriesSeason Season { get; init; } = SeriesSeason.Default;
    public DateTimeOffset SubscribedAt { get; init; }

    public UserSubscription(string userId, int seriesId)
    {
        UserId = userId;
        SeriesId = seriesId;
        Id = $"subscription:{seriesId}";
    }
}
