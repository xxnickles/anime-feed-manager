using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.FeedManager.Completion;

public abstract record CompletionOutcome;

public sealed record FeedCompleted(DateTimeOffset At) : CompletionOutcome;

public sealed record FeedOpen : CompletionOutcome;

public static class NyaaCompletion
{
    /// <summary>Completes once, when the distinct regular episodes reach a known count, at the latest regular release; only a rebind clears the stamp.</summary>
    /// <remarks>
    /// Counted, not the highest number: groups can number a sequel on from earlier seasons. Without a premiere the
    /// catalog can hold earlier seasons too, so it never completes.
    /// </remarks>
    public static CompletionOutcome Evaluate(
        ExpectedEpisodes expected,
        DateTimeOffset? premiere,
        DateTimeOffset? completedAt,
        IReadOnlyCollection<NyaaCatalogStorage> catalog) =>
        (expected, premiere, completedAt) is (KnownEpisodes {Count: > 0} known, not null, null)
        && catalog.DistinctRegular() >= known.Count
            ? new FeedCompleted(catalog.Where(row => !row.Special).Max(row => row.Published))
            : new FeedOpen();

    /// <summary>Stamps a completed binding; the library completes when the binding retires.</summary>
    public static Task<Result<CompletionOutcome>> CompleteWhenReached(
        string seriesId,
        SeriesSeason season,
        NyaaBindingGetter bindingGetter,
        NyaaCatalogGetter catalogGetter,
        NyaaBindingUpdater bindingUpdater,
        CancellationToken token) =>
        OptimisticConcurrency.UntilWritten(
                attemptToken => bindingGetter(seriesId, season, attemptToken)
                    .Bind(binding => catalogGetter(seriesId, attemptToken)
                        .Map(catalog => (Binding: binding, Outcome: Evaluate(
                            SeriesBroadcast.ToExpectedEpisodes(binding.Episodes),
                            binding.Premiere,
                            binding.CompletedAt,
                            catalog))))
                    .Bind(state => state.Outcome is FeedCompleted completed
                        ? bindingUpdater(Stamp(state.Binding, completed.At), attemptToken).Map(_ => state.Outcome)
                        : Task.FromResult(Result<CompletionOutcome>.Success(state.Outcome))),
                token,
                error => error is PreconditionFailedError)
            .WithOperationName(nameof(CompleteWhenReached))
            .WithLogProperty(Telemetry.SeriesIdTag, seriesId);

    private static NyaaBindingStorage Stamp(NyaaBindingStorage binding, DateTimeOffset at)
    {
        binding.CompletedAt = at;
        return binding;
    }
}
