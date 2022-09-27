using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Interface;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvSubscription;

public class EnqueueSubscribersToNotify
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    private readonly ILogger<EnqueueSubscribersToNotify> _logger;

    public EnqueueSubscribersToNotify(ISubscriptionRepository subscriptionRepository, ILoggerFactory loggerFactory)
    {
        _subscriptionRepository = subscriptionRepository;
        _logger = loggerFactory.CreateLogger<EnqueueSubscribersToNotify>();
    }

    [Function("EnqueueSubscribersToNotify")]
    [QueueOutput(QueueNames.SubscribersToProcess, Connection = "AzureWebJobsStorage")]
    public async Task<IEnumerable<string>> Run(
        [TimerTrigger("0 0 * * * *")] TimerInfo timer
    )
    {
        var subscribers = await _subscriptionRepository.GetAllSubscribers();
        return subscribers.Match(
            subs =>
            {
                _logger.LogInformation("Processing following subscribers {Subscribers}", string.Join(", ", subs));
                return subs;
            },
            e =>
            {
                _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message);
                return Enumerable.Empty<string>();
            });
    }
}