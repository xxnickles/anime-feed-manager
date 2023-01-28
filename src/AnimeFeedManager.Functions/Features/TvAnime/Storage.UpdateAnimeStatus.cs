using System.Collections.Immutable;
using AnimeFeedManager.Application.TvAnimeLibrary.Commands;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnime;

public class UpdateAnimeStatusMessages
{
    [QueueOutput(QueueNames.TvAnimeLibraryUpdates)]
    public IEnumerable<string>? AnimeMessages { get; set; }

    [QueueOutput(QueueNames.ProcessAutoSubscriber, Connection = "AzureWebJobsStorage")]
    public string? AutoSubscribeMessages { get; set; }
}

public class UpdateAnimeStatus
{
    private readonly IUpdateState _updateState;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateAnimeStatus> _logger;

    public UpdateAnimeStatus(
        IUpdateState updateState,
        IMediator mediator,
        ILoggerFactory loggerFactory)
    {
        _updateState = updateState;
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<UpdateAnimeStatus>();
    }

    [Function("UpdateAnimeStatus")]
    public async Task<UpdateAnimeStatusMessages> Run(
        [QueueTrigger(QueueNames.TitleProcess, Connection = "AzureWebJobsStorage")]
        string processResult
    )
    {
        if (processResult == ProcessResult.Ok)
        {
            _logger.LogInformation(
                "Titles source has been updated. Verifying whether series need to be marked as completed");

            var result = await _mediator.Send(new UpdateStatusCmd()).BindAsync(AddState);
            return result.Match(
                v => new UpdateAnimeStatusMessages
                {
                    AnimeMessages = v.Select(Serializer.ToJson),
                    AutoSubscribeMessages = ProcessResult.Ok
                },
                e =>
                {
                    _logger.LogError("[{ArgCorrelationId}]: {ArgMessage}", e.CorrelationId, e.Message);
                    return new UpdateAnimeStatusMessages
                    {
                        AutoSubscribeMessages = ProcessResult.Failure
                    };
                });
        }

        _logger.LogWarning("Title process failed, series status is not going to be updated");
        return new UpdateAnimeStatusMessages
        {
            AutoSubscribeMessages = ProcessResult.NoChanges
        };
    }

    private Task<Either<DomainError, ImmutableList<StateWrapper<AnimeInfoStorage>>>> AddState(
        ImmutableList<AnimeInfoStorage> animes)
    {
        return _updateState.Create(NotificationFor.Tv, animes.Count)
            .MapAsync(id => animes.ConvertAll(a => new StateWrapper<AnimeInfoStorage>(id, a)));
    }
}