using System.Collections.Immutable;
using AnimeFeedManager.Application.OvasLibrary.Queries;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications.Realtime;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Ovas;

public class ScrapOvasLibraryOutput
{
    [QueueOutput(QueueNames.OvasLibraryUpdates)]
    public IEnumerable<string>? AnimeMessages { get; set; }

    [QueueOutput(QueueNames.ImageProcess)] public IEnumerable<string>? ImagesMessages { get; set; }
}

public class ScrapOvasLibrary
{
    private record struct StateLibraryForStorage(
        ImmutableList<StateWrapper<OvaStorage>> Ovas,
        ImmutableList<StateWrapper<BlobImageInfoEvent>> Images,
        SeasonInfoDto Season
    );

    private readonly IUpdateState _updateState;
    private readonly IDomainPostman _domainPostman;
    private readonly IMediator _mediator;
    private readonly ILogger<ScrapOvasLibrary> _logger;

    public ScrapOvasLibrary(
        IUpdateState updateState,
        IDomainPostman domainPostman,
        IMediator mediator,
        ILoggerFactory loggerFactory)
    {
        _updateState = updateState;
        _domainPostman = domainPostman;
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<ScrapOvasLibrary>();
    }

    [Function("ScrapOvasLibrary")]
    public async Task<ScrapOvasLibraryOutput> Run(
        [QueueTrigger(QueueNames.OvasLibraryUpdate, Connection = "AzureWebJobsStorage")]
        OvasUpdate payload)
    {
        _logger.LogInformation("Processing update of the full ovas library");

        var result = await RunCommand(payload).BindAsync(AddState);


        return result.Match(
            result => new ScrapOvasLibraryOutput
            {
                AnimeMessages = result.Ovas.Select(Serializer.ToJson),
                ImagesMessages = result.Images.Select(Serializer.ToJson)
            },
            e =>
            {
                _logger.LogError("An error occurred while processing library update {S}", e.ToString());
                _domainPostman.SendMessage(new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Error,
                    new NullSeasonInfo(),
                    SeriesType.Ova,
                    "An error occurred before storing ovas."));

                return new ScrapOvasLibraryOutput
                {
                    AnimeMessages = null,
                    ImagesMessages = null
                };
            });
    }

    private Task<Either<DomainError, OvasLibraryForStorage>> RunCommand(OvasUpdate command)
    {
        return command.Type switch
        {
            ShortSeriesUpdateType.Latest => _mediator.Send(new GetOvasLibraryQry()),
            ShortSeriesUpdateType.Season => _mediator.Send(new GetScrappedOvasLibraryQry(command.SeasonInformation!)),
            _ => throw new ArgumentOutOfRangeException(nameof(command.Type), "Ova update type has is invalid")
        };
    }

    private Task<Either<DomainError, StateLibraryForStorage>> AddState(OvasLibraryForStorage library)
    {
        Task<Either<DomainError, (string seriesStateId, string imagesStateId)>> CombineIds(string seriesStateId)
        {
            return _updateState.Create(Common.Notifications.NotificationType.Images, library.Images.Count)
                .MapAsync(imgIds => (seriesStateId, imgIds));
        }

        return _updateState.Create(Common.Notifications.NotificationType.Ova, library.Ovas.Count)
            .BindAsync(CombineIds)
            .MapAsync(r => new StateLibraryForStorage(
                library.Ovas.ConvertAll(a => new StateWrapper<OvaStorage>(r.seriesStateId, a)),
                library.Images.ConvertAll(i => new StateWrapper<BlobImageInfoEvent>(r.imagesStateId, i)),
                library.Season
            ));
    }
    
}