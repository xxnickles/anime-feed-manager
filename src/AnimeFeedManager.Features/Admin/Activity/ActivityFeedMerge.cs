namespace AnimeFeedManager.Features.Admin.Activity;

/// <summary>
/// Combines any number of persisted-event buckets into one feed: newest first, capped at
/// <paramref name="take"/> overall. Kept separate from the Cosmos reads so it's plainly unit
/// testable — each bucket already did its own per-source capping at the storage layer; this only
/// merges and re-caps across buckets. Params-based so a future bucket slots in as another argument.
/// </summary>
public static class ActivityFeedMerge
{
    public static ImmutableArray<IPersistedEvent> Merge(int take, params IEnumerable<IPersistedEvent>[] buckets) =>
        [.. buckets.SelectMany(bucket => bucket).OrderByDescending(item => item.OccurredAt).Take(take)];
}
