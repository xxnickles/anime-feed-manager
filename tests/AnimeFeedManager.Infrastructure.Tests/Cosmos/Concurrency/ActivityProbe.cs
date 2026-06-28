using System.Diagnostics;
using AnimeFeedManager.Infrastructure.Cosmos.Concurrency;

namespace AnimeFeedManager.Infrastructure.Tests.Cosmos.Concurrency;

/// <summary>
/// Makes an <see cref="Activity"/> current and recording for the duration of a test, so code
/// that stamps tags on <see cref="Activity.Current"/> (the retry counter in
/// <see cref="OptimisticConcurrency"/>) can be observed. Without a registered listener
/// <c>StartActivity</c> returns null and the tag writes silently no-op.
/// </summary>
internal sealed class ActivityProbe : IDisposable
{
    private readonly ActivitySource _source;
    private readonly ActivityListener _listener;

    public ActivityProbe()
    {
        _source = new ActivitySource("untilwritten-tests");
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source == _source,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(_listener);
        Activity = _source.StartActivity("probe")!;
    }

    public Activity Activity { get; }

    /// <summary>The accumulated retry count stamped by <see cref="OptimisticConcurrency"/>, or null when none.</summary>
    public int? Retries => Activity.GetTagItem(OptimisticConcurrency.PreconditionRetriesTag) as int?;

    public void Dispose()
    {
        Activity.Dispose();
        _listener.Dispose();
        _source.Dispose();
    }
}
