using AnimeFeedManager.Features.SystemEvents.Storage.Stores;

namespace AnimeFeedManager.Features.SystemEvents.Stats;

public sealed record ErrorCount(string PayloadTypeName, int Count, DateTimeOffset Latest);

public sealed record ErrorStats(ImmutableArray<ErrorCount> BySource)
{
    public int Total => BySource.Sum(c => c.Count);
}

public static class ErrorStat
{
    // Errors are only stored under the Admin and Everybody partitions; querying each avoids a full-table scan.
    public static Task<Result<ErrorStats>> Get(
        SystemErrorsGetter errorsGetter,
        DateTimeOffset from,
        CancellationToken cancellationToken) =>
        errorsGetter(TargetConsumer.Admin(), from, cancellationToken)
            .Bind(admin => errorsGetter(TargetConsumer.Everybody(), from, cancellationToken)
                .Map(admin.AddRange))
            .Map(Summarize);

    internal static ErrorStats Summarize(ImmutableArray<EventOccurrence> errors) =>
        new([
            .. errors
                .GroupBy(e => e.PayloadTypeName)
                .Select(g => new ErrorCount(g.Key, g.Count(), g.Max(e => e.Timestamp)))
                .OrderByDescending(c => c.Count)
                .ThenByDescending(c => c.Latest)
        ]);
}
