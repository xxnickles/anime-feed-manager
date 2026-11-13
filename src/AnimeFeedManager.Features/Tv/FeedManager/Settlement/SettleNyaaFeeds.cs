using AnimeFeedManager.Features.Tv.FeedManager.Completion;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Settlement;

public sealed record FeedsSettled(int Stamped, int Retired);

public static class SettleNyaaFeeds
{
    /// <summary>Window for late revisions and batches between a feed's completion and its retirement.</summary>
    internal static readonly TimeSpan RetireAfter = TimeSpan.FromDays(6);

    /// <summary>Stamps tracked feeds that reached their count, then retires feeds completed <see cref="RetireAfter"/> ago; a series that fails is logged and retried next run.</summary>
    public static Task<Result<FeedsSettled, DomainError>> Run(
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        CancellationToken token) =>
        CompleteFeeds(clientFactory, token)
            .Bind(stamped => RetireFeeds(clientFactory, domainPostman, DateTimeOffset.UtcNow, token)
                .Map(retired => new FeedsSettled(stamped, retired)))
            .WithOperationName(nameof(Run));

    internal static ImmutableArray<NyaaBindingStorage> ToComplete(IEnumerable<NyaaBindingStorage> tracked) =>
        [.. tracked.Where(binding => binding is {Tracking: true, CompletedAt: null, Premiere: not null, Episodes: > 0})];

    internal static ImmutableArray<NyaaBindingStorage> ToRetire(IEnumerable<NyaaBindingStorage> tracked,
        DateTimeOffset now) =>
        [.. tracked.Where(binding => binding is {Tracking: true, CompletedAt: { } completedAt} && completedAt <= now - RetireAfter)];

    private static Task<Result<int, DomainError>> CompleteFeeds(ITableClientFactory clientFactory, CancellationToken token) =>
        clientFactory.TableStorageTrackedNyaaBindings(token)
            .MapError(error => (DomainError)error)
            .Bind(tracked => Each(ToComplete(tracked), binding => Complete(binding, clientFactory, token)))
            .WithOperationName(nameof(CompleteFeeds));

    // Events before untracking: a failed untrack re-sends next run, and library completion is idempotent.
    private static Task<Result<int, DomainError>> RetireFeeds(
        ITableClientFactory clientFactory,
        DomainCollectionSender domainPostman,
        DateTimeOffset now,
        CancellationToken token) =>
        clientFactory.TableStorageTrackedNyaaBindings(token)
            .MapError(error => (DomainError)error)
            .Map(tracked => ToRetire(tracked, now))
            .BindWhen(
                due => domainPostman([.. due.Select(binding => new NyaaFeedCompleted(binding.RowKey ?? string.Empty))], token)
                    .Map(_ => due),
                due => !due.IsEmpty)
            .Bind(due => Each(due, binding => clientFactory.TableStorageNyaaBindingUpdater(Untracked(binding), token)
                .Map(_ => true)))
            .WithOperationName(nameof(RetireFeeds));

    private static Task<Result<bool, DomainError>> Complete(NyaaBindingStorage binding, ITableClientFactory clientFactory,
        CancellationToken token) =>
        IdHelpers.SeriesSeasonFromId(binding.RowKey ?? string.Empty)
            .MapError(error => (DomainError)error)
            .Bind(season => NyaaCompletion.CompleteWhenReached(
                binding.RowKey ?? string.Empty,
                season,
                clientFactory.TableStorageNyaaBindingGetter,
                clientFactory.TableStorageNyaaCatalogGetter,
                clientFactory.TableStorageNyaaBindingUpdater,
                token))
            .Map(outcome => outcome is FeedCompleted);

    /// <summary>Counts the series settled; failures are logged without failing the step, even when all fail.</summary>
    private static async Task<Result<int, DomainError>> Each<TError>(
        ImmutableArray<NyaaBindingStorage> bindings,
        Func<NyaaBindingStorage, Task<Result<bool, TError>>> settle) where TError : DomainError =>
        (await Task.WhenAll(bindings.Select(settle)))
        .Flatten(settled => settled.Count(done => done))
        .AddLogOnSuccess(bulk => bulk.LogErrors)
        .Map(bulk => bulk.Value)
        .BindOnErrorWhen(
            error => Result<int, DomainError>.Success(0).AddLog(error.WriteError),
            error => error is AggregatedError);

    private static NyaaBindingStorage Untracked(NyaaBindingStorage binding)
    {
        binding.Tracking = false;
        return binding;
    }
}
