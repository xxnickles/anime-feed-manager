using AnimeFeedManager.Application.OvasSubscriptions.Commands;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.OvasSubscriptions;

public class PersistSeries
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersistSeries> _logger;

    public PersistSeries(
        IMediator mediator,
        ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<PersistSeries>();
    }


    [Function("CompleteOvaNotification")]
    public async Task Run(
        [QueueTrigger(QueueNames.OvasMarkCompletedProcess, Connection = "AzureWebJobsStorage")]
        ShortSeriesUpdate completedOva
    )
    {
        _logger.LogInformation("Completing {Title} for {Subscriber}", completedOva.Series, completedOva.User);
     
        var result = await _mediator.Send(new CompleteSubscriptionCmd(completedOva.User, completedOva.Series));
   
        result.Match( 
           _ => _logger.LogInformation("{Title} for {Subscriber} has been completed", completedOva.Series, completedOva.User),
           e => _logger.LogError("[{CorrelationId}] An error occurred when completing OVA {Title} for {Subscriber}: {Error}",
               e.CorrelationId, completedOva.Series, completedOva.User, e.Message)
       );
    }

}