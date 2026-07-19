using AnimeFeedManager.Features.Feeds.Entities;

namespace AnimeFeedManager.Features.Feeds.Events;

/// <summary>
/// Raised once a Nyaa collection run (hot or cold path) completes with at least one match —
/// a fire-and-forget domain event fed to the admin SSE toast. Success-shaped only, matching
/// <c>SeasonImported</c>'s precedent; a run with no matches stays silent.
/// </summary>
public sealed record NyaaCollectionRunCompleted(
    CollectionSource Source, int ItemsScanned, int MatchedCount, int UnmatchedCount);
