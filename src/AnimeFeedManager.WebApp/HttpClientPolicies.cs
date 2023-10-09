using Polly;
using Polly.Extensions.Http;

namespace AnimeFeedManager.WebApp;

public static class HttpClientPolicies
{
    private static readonly Random Jitterer = new();

    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider, int retryCount = 3) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                + TimeSpan.FromMilliseconds(Jitterer.Next(0, 100)),
                onRetry: (result, span, index, ctx) =>
                {
                    Console.WriteLine($"[Form console] tentative #{index}, received {result.Result.StatusCode}, retrying...");
                    var logger = serviceProvider.GetService<ILogger>();
                    logger?.LogWarning("tentative #{Index}, received {ResultStatusCode}, retrying...", index, result.Result.StatusCode);
                });
}