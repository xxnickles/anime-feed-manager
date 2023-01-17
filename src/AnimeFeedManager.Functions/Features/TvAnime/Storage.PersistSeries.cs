using AnimeFeedManager.Application.State.Commands;
using AnimeFeedManager.Application.TvAnimeLibrary.Commands;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnime;

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


    [Function("PersistTvSeries")]
    public async Task Run(
        [QueueTrigger(QueueNames.TvAnimeLibraryUpdates, Connection = "AzureWebJobsStorage")]
        StateWrapper<AnimeInfoStorage> animeInfoState
    )
    {
        _logger.LogInformation("storing {AnimeInfoTitle}", animeInfoState.Payload.Title);
        var season = new SeasonInfoDto(animeInfoState.Payload.Season ?? string.Empty, animeInfoState.Payload.Year);
        var command = new MergeAnimeInfoCmd(animeInfoState.Payload);
        var result = await _mediator.Send(command);

       var stateResult = await result.MatchAsync(
            _ => OnSuccess(animeInfoState.Payload.Title ?? string.Empty, animeInfoState.Id, season),
            e => OnError(animeInfoState.Payload.Title ?? string.Empty, e, animeInfoState.Id, season)

        );

       stateResult.Match( 
           _ => _logger.LogInformation("[{Caller}] State has been updated", "PersistTvSeries"),
           e => _logger.LogError("[{CorrelationId}] An error occurred when updating state for the Tv series {Title}: {Error}",
               e.CorrelationId, animeInfoState.Payload.Title, e.Message)
       );

    }


    private Task<Either<DomainError, LanguageExt.Unit>> OnSuccess(
        string seriesTitle,
        string stateId, 
        SeasonInfoDto seasonInfoDto)
    {
         _logger.LogInformation("Series '{AnimeInfoTitle}' has been stored", seriesTitle);
        return _mediator.Send(new UpdateTvScrapStateCmd(stateId, UpdateType.Complete, seasonInfoDto));
    }

    private Task<Either<DomainError, LanguageExt.Unit>> OnError(
        string seriesTitle,
        DomainError error,
        string stateId, 
        SeasonInfoDto seasonInfoDto)
    {
        _logger.LogError("[{CorrelationId} / {Title}]: {Message} ", error.CorrelationId, seriesTitle, error.Message);
        return _mediator.Send(new UpdateTvScrapStateCmd(stateId, UpdateType.Error, seasonInfoDto));
    }
}