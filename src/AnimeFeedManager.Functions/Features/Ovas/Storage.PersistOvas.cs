using AnimeFeedManager.Application.OvasLibrary.Commands;
using AnimeFeedManager.Application.State.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Ovas;

public class PersistOvas
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersistOvas> _logger;

    public PersistOvas(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<PersistOvas>();
    }


    [Function("PersistOvas")]
    public async Task Run(
        [QueueTrigger(QueueNames.OvasLibraryUpdates, Connection = "AzureWebJobsStorage")] StateWrapper<OvaStorage> ovaStorage
        )
    {
        _logger.LogInformation("storing {AnimeInfoTitle}", ovaStorage.Payload.Title);
        var season = new SeasonInfoDto(ovaStorage.Payload.Season ?? string.Empty, ovaStorage.Payload.Year);
        var command = new MergeOvaCmd(ovaStorage.Payload);
        var result = await _mediator.Send(command);
        
        var stateResult = await result.MatchAsync(
            _ => OnSuccess(ovaStorage.Payload.Title ?? string.Empty, ovaStorage.Id, season),
            e => OnError(ovaStorage.Payload.Title ?? string.Empty, e, ovaStorage.Id, season)

        );
        
        
        result.Match(
            _ => _logger.LogInformation("Ova '{OvaTitle}' has been stored", ovaStorage.Payload.Title),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
    
    private Task<Either<DomainError, LanguageExt.Unit>> OnSuccess(
        string seriesTitle,
        string stateId, 
        SeasonInfoDto seasonInfoDto)
    {
        _logger.LogInformation("Series '{AnimeInfoTitle}' has been stored", seriesTitle);
        return _mediator.Send(new UpdateOvaScrapStateCmd(stateId, UpdateType.Complete, seasonInfoDto));
    }

    private Task<Either<DomainError, LanguageExt.Unit>> OnError(
        string seriesTitle,
        DomainError error,
        string stateId, 
        SeasonInfoDto seasonInfoDto)
    {
        _logger.LogError("[{CorrelationId} / {Title}]: {Message} ", error.CorrelationId, seriesTitle, error.Message);
        return _mediator.Send(new UpdateOvaScrapStateCmd(stateId, UpdateType.Error, seasonInfoDto));
    }
}