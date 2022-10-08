using AnimeFeedManager.Application.OvasLibrary.Queries;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Storage.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Ovas;

public class ScrapOvasLibraryOutput
{
    [QueueOutput(QueueNames.OvasLibraryUpdates)]
    public IEnumerable<string>? AnimeMessages { get; set; }

    [QueueOutput(QueueNames.ImageProcess)] public IEnumerable<string>? ImagesMessages { get; set; }

    [QueueOutput(QueueNames.AvailableSeasonsUpdates)]
    public string? SeasonMessage { get; set; }
}

public class ScrapOvasLibrary
{
    private readonly IDomainPostman _domainPostman;
    private readonly IFeedProvider _feedProvider;
    private readonly IMediator _mediator;
    private readonly ILogger<ScrapOvasLibrary> _logger;

    public ScrapOvasLibrary(
        IDomainPostman domainPostman,
        IMediator mediator,
        ILoggerFactory loggerFactory)
    {
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

        var result = await RunCommand(payload);


        return result.Match(
            result =>
            {
                
                _domainPostman.SendMessage(new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Information,
                    result.Season,
                    SeriesType.Ova,
                    $"{result.Ovas.Count} ovas of {result.Season.Season}-{result.Season.Year} will be stored"));

                return new ScrapOvasLibraryOutput
                {
                    AnimeMessages = result.Ovas.Select(Serializer.ToJson),
                    ImagesMessages = result.Images.Select(Serializer.ToJson),
                    SeasonMessage = Serializer.ToJson(result.Season),
                };
            },
            e =>
            {
                _logger.LogError("An error occurred while processing library update {S}", e.ToString());
                _domainPostman.SendMessage(new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Error,
                    new NullSeasonInfo(),
                    SeriesType.Tv,
                    $"An error occurred before storing ovas."));

                return new ScrapOvasLibraryOutput
                {
                    AnimeMessages = null,
                    ImagesMessages = null,
                    SeasonMessage = null
                };
            });
    }

    private Task<Either<DomainError, OvasLibraryForStorage>> RunCommand(OvasUpdate command)
    {
        return command.Type switch
        {
            ShortSeriesUpdateType.Latest => _mediator.Send(new GetOvasLibraryQry()),
            ShortSeriesUpdateType.Season => _mediator.Send(new GetScrappedOvasLibraryQry(command.SeasonInformation)),
            _ => throw new ArgumentOutOfRangeException(nameof(command.Type), "Ova update type has is invalid")
        };
    }
  
}