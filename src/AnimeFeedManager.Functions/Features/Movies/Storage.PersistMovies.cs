using AnimeFeedManager.Application.MoviesLibrary.Commands;
using AnimeFeedManager.Application.State.Commands;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Notifications;
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
        [QueueTrigger(QueueNames.MoviesLibraryUpdates, Connection = "AzureWebJobsStorage")] StateWrapper<MovieStorage> movieStorage
        )
    {
        _logger.LogInformation("storing {AnimeInfoTitle}", movieStorage.Payload.Title);
        var season = new SeasonInfoDto(movieStorage.Payload.Season ?? string.Empty, movieStorage.Payload.Year);
        var command = new MergeMovieCmd(movieStorage.Payload);
        var result = await _mediator.Send(command);
        
        var stateResult = await result.MatchAsync(
            _ => OnSuccess(movieStorage.Payload.Title ?? string.Empty, movieStorage.Id, season),
            e => OnError(movieStorage.Payload.Title ?? string.Empty, e, movieStorage.Id, season)

        );
        
        
        result.Match(
            _ => _logger.LogInformation("Movie '{MovieTitle}' has been stored", movieStorage.Payload.Title),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
    
    private Task<Either<DomainError, LanguageExt.Unit>> OnSuccess(
        string seriesTitle,
        string stateId, 
        SeasonInfoDto seasonInfoDto)
    {
        _logger.LogInformation("Series '{AnimeInfoTitle}' has been stored", seriesTitle);
        return _mediator.Send(new UpdateMovieScrapStateCmd(stateId, UpdateType.Complete, seasonInfoDto));
    }

    private Task<Either<DomainError, LanguageExt.Unit>> OnError(
        string seriesTitle,
        DomainError error,
        string stateId, 
        SeasonInfoDto seasonInfoDto)
    {
        _logger.LogError("[{CorrelationId} / {Title}]: {Message} ", error.CorrelationId, seriesTitle, error.Message);
        return _mediator.Send(new UpdateMovieScrapStateCmd(stateId, UpdateType.Error, seasonInfoDto));
    }
}