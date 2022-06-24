using System.Threading.Tasks;
using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Core.Dto;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class ProcessAutomaticSubscription
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessAutomaticSubscription> _logger;

    public ProcessAutomaticSubscription(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<ProcessAutomaticSubscription>();
    }

    [Function("ProcessAutomaticSubscription")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task Run([QueueTrigger(QueueNames.ToSubscribe)] SubscriptionDto newSubscription)
    {
        _logger.LogInformation("Automated subscription to {SubscriptionSeries} for user {UserId}", newSubscription.Series, newSubscription.UserId);
        var command = new MergeSubscription(newSubscription.UserId, newSubscription.Series);
        var result = await _mediator.Send(command);
        result.Match(
            _ => _logger.LogInformation("{UserId} has subscribed to {Series} automatically", newSubscription.UserId, newSubscription.Series),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}