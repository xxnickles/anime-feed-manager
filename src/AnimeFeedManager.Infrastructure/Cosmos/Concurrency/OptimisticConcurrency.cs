using System.Diagnostics;
using System.Net;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared.Results.Static;

namespace AnimeFeedManager.Infrastructure.Cosmos.Concurrency;

/// <summary>
/// Optimistic-concurrency retry policy for Cosmos point writes guarded by ETag. Repeats the supplied
/// read-modify-write until it succeeds, fails with a status outside <paramref name="retryStatuses"/>,
/// or cancellation is signaled. The retryable conflict statuses are supplied explicitly by each caller —
/// there is no implicit default — so the retry surface is visible at every call site.
/// <para>
/// Typical sets: <see cref="HttpStatusCode.PreconditionFailed"/> (412 — lost ETag race on a replace)
/// for replace/upsert paths; add <see cref="HttpStatusCode.Conflict"/> (409 — lost create race) for
/// create-then-replace paths, where re-reading converges into the now-existing record. A status not in
/// the set is returned as-is, never retried. Cancellation is the only escape from sustained contention.
/// </para>
/// </summary>
public static class OptimisticConcurrency
{
    /// <summary>
    /// Activity tag carrying the number of write-conflict retries a single <see cref="UntilWritten{T}"/>
    /// run incurred. Accumulated onto <see cref="Activity.Current"/> so multiple guarded writes within
    /// the same span sum into one contention count. Absent when no retry occurred.
    /// </summary>
    public const string PreconditionRetriesTag = "cosmos.precondition_retries";

    /// <summary>
    /// Invokes <paramref name="readModifyWrite"/>, retrying while the returned failure is a
    /// <see cref="CosmosResponseError"/> whose <c>StatusCode</c> is in <paramref name="retryStatuses"/>.
    /// Any other outcome — success, or an error outside the set — is returned to the caller as-is.
    /// On exit, the retry count (when non-zero) is accumulated onto <see cref="Activity.Current"/> under
    /// <see cref="PreconditionRetriesTag"/>, so write-conflict storms stay queryable on the span.
    /// </summary>
    public static async Task<Result<T>> UntilWritten<T>(
        Func<CancellationToken, Task<Result<T>>> readModifyWrite,
        CancellationToken cancellationToken,
        HttpStatusCode[] retryStatuses)
    {
        var retries = 0;
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await readModifyWrite(cancellationToken);
                if (!IsRetryable(result, retryStatuses)) return result;
                retries++;
            }
        }
        finally
        {
            if (retries > 0 && Activity.Current is { } activity)
            {
                var prior = activity.GetTagItem(PreconditionRetriesTag) as int? ?? 0;
                activity.SetTag(PreconditionRetriesTag, prior + retries);
            }
        }
    }

    private static bool IsRetryable<T>(Result<T> result, HttpStatusCode[] retryStatuses) =>
        result.MatchToValue(
            onOk: _ => false,
            onError: err => err is CosmosResponseError { StatusCode: var status } && retryStatuses.Contains(status));
}
