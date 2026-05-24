using Polly;
using Polly.Retry;

namespace AnimeFeedManager.Infrastructure.Background.Queue;

/// <summary>
/// Shared default Polly pipeline used when a <see cref="WorkHandler{T}"/> does not
/// supply its own. 3 retries, exponential backoff with jitter, 1-second base
/// delay. Polly's default <c>ShouldHandle</c> retries all exceptions except
/// <see cref="OperationCanceledException"/>, which keeps shutdown clean.
/// </summary>
internal static class WorkQueueDefaults
{
    internal static ResiliencePipeline BuildDefaultPipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
            })
            .Build();
}
