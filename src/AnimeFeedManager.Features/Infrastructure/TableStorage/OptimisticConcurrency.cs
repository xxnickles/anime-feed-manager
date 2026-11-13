namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

public static class OptimisticConcurrency
{
    /// <summary>
    /// Repeats <paramref name="readModifyWrite"/> while its error satisfies <paramref name="retryWhen"/>;
    /// any other outcome is returned as-is. Cancellation is the only escape from sustained contention.
    /// </summary>
    /// <remarks>Each attempt must re-read and re-derive its change; a decision carried over from a lost attempt is stale.</remarks>
    public static async Task<Result<T, TError>> UntilWritten<T, TError>(
        Func<CancellationToken, Task<Result<T, TError>>> readModifyWrite,
        CancellationToken token,
        Func<TError, bool> retryWhen)
    {
        var retries = 0;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var result = await readModifyWrite(token);
            if (!result.MatchToValue(_ => false, retryWhen))
                return retries > 0 ? result.WithLogProperty(Telemetry.WriteRetriesTag, retries) : result;
            retries++;
        }
    }
}
