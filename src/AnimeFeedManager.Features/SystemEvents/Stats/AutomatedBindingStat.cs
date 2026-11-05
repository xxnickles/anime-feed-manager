using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

namespace AnimeFeedManager.Features.SystemEvents.Stats;

public abstract record LastAutomatedBinding;

public sealed record NoAutomatedBinding : LastAutomatedBinding;

public sealed record LatestAutomatedBinding(AutomatedBindingCompleted Run) : LastAutomatedBinding;

public static class AutomatedBindingStat
{
    public static Task<Result<LastAutomatedBinding>> Get(LatestAutomatedBindingRun runGetter,
        CancellationToken cancellationToken) =>
        runGetter(cancellationToken)
            .Map(LastAutomatedBinding (run) => new LatestAutomatedBinding(run))
            .BindOnErrorWhen(_ => new NoAutomatedBinding(), error => error is NotFoundError);
}
