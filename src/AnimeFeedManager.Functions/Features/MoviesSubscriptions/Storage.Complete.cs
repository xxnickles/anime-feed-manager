using AnimeFeedManager.Application.MoviesSubscriptions.Commands;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.MoviesSubscriptions;

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


    [Function("CompleteMovieNotification")]
    public async Task Run(
        [QueueTrigger(QueueNames.MoviesMarkCompletedProcess, Connection = "AzureWebJobsStorage")]
        ShortSeriesUpdate completedMovie
    )
    {
        _logger.LogInformation("Completing {Title} for {Subscriber}", completedMovie.Series, completedMovie.User);
     
        var result = await _mediator.Send(new CompleteSubscriptionCmd(completedMovie.User, completedMovie.Series));
   
        result.Match( 
           _ => _logger.LogInformation("{Title} for {Subscriber} has been completed", completedMovie.Series, completedMovie.User),
           e => _logger.LogError("[{CorrelationId}] An error occurred when completing Movie {Title} for {Subscriber}: {Error}",
               e.CorrelationId, completedMovie.Series, completedMovie.User, e.Message)
       );
    }

}