using AnimeFeedManager.Application.OvasSubscriptions.Queries;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.OvasSubscriptions;

public class EnqueueSubscribersToNotify
{
    private readonly IMediator _mediator;
    private readonly ILogger<EnqueueSubscribersToNotify> _logger;

    public EnqueueSubscribersToNotify(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<EnqueueSubscribersToNotify>();
    }

    [Function("EnqueueOvasSubscribersToNotify")]
    [QueueOutput(QueueNames.OvasSubscriptionsToProcess, Connection = "AzureWebJobsStorage")]
    public async Task<IEnumerable<string>> Run(
        [TimerTrigger("0 0 0 18 * *")] TimerInfo timer
    )
    {
        var subscribers = await _mediator.Send(new GetTodaySubscriptionsQry());
        return subscribers.Match(
            subs =>
            {
                _logger.LogInformation("Processing following OVAs subscribers {Subscribers}", string.Join(", ", subs.Select(x => x.Subscriber)));
                return subs.Select(Serializer.ToJson);
            },
            e =>
            {
                _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message);
                return Enumerable.Empty<string>();
            });
    }
}