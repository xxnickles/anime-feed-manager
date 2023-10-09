using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public sealed class OnUserAutoSubscription
{
    private readonly InterestedToSubscribe _interestedToSubscribe;
    private readonly ILogger<OnUserAutoSubscription> _logger;

    public OnUserAutoSubscription(
        InterestedToSubscribe interestedToSubscribe,
        ILoggerFactory loggerFactory)
    {
        _interestedToSubscribe = interestedToSubscribe;
        _logger = loggerFactory.CreateLogger<OnUserAutoSubscription>();
    }

    [Function("OnUserAutoSubscription")]
    public async Task Run(
        [QueueTrigger(Box.Available.UserAutoSubscriptionBox, Connection = "AzureWebJobsStorage")]
        UserAutoSubscription notification)
    {
        var result = await UserIdValidator.Validate(notification.UserId)
            .ValidationToEither()
            .BindAsync(user => _interestedToSubscribe.ProcessInterested(user, default));

        result.Match(
            count => _logger.LogInformation("{Count} automated subscriptions will be processed for {User}", count,
                notification.UserId),
            error => error.LogDomainError(_logger));
    }
}