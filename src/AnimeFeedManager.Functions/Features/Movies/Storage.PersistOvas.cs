using AnimeFeedManager.Application.MoviesLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Movies;

public class PersistMovies
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersistMovies> _logger;

    public PersistMovies(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<PersistMovies>();
    }


    [Function("PersistMovies")]
    public async Task Run(
        [QueueTrigger(QueueNames.MoviesLibraryUpdates, Connection = "AzureWebJobsStorage")] MovieStorage movieStorage
        )
    {
        _logger.LogInformation("storing {AnimeInfoTitle}", movieStorage.Title);
        var command = new MergeMovieCmd(movieStorage);
        var result = await _mediator.Send(command);
        result.Match(
            _ => _logger.LogInformation("Movie '{MovieTitle}' has been stored", movieStorage.Title),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}