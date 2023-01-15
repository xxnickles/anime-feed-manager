using AnimeFeedManager.Application.MoviesLibrary.Queries;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications.Realtime;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Movies;

public class ScrapMoviesLibraryOutput
{
    [QueueOutput(QueueNames.MoviesLibraryUpdates)]
    public IEnumerable<string>? AnimeMessages { get; set; }

    [QueueOutput(QueueNames.ImageProcess)] public IEnumerable<string>? ImagesMessages { get; set; }
}

public class ScrapMoviesLibrary
{
    private readonly IDomainPostman _domainPostman;
    private readonly IMediator _mediator;
    private readonly ILogger<ScrapMoviesLibrary> _logger;

    public ScrapMoviesLibrary(
        IDomainPostman domainPostman,
        IMediator mediator,
        ILoggerFactory loggerFactory)
    {
        _domainPostman = domainPostman;
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<ScrapMoviesLibrary>();
    }

    [Function("ScrapMoviesLibrary")]
    public async Task<ScrapMoviesLibraryOutput> Run(
        [QueueTrigger(QueueNames.MoviesLibraryUpdate, Connection = "AzureWebJobsStorage")]
        MoviesUpdate payload)
    {
        _logger.LogInformation("Processing update of the full Movies library");

        var result = await RunCommand(payload);


        return result.Match(
            result =>
            {
                _domainPostman.SendMessage(new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Information,
                    result.Season,
                    SeriesType.Movie,
                    $"{result.Movies.Count} Movies of {result.Season.Season}-{result.Season.Year} will be stored"));

                _domainPostman.SendDelayedMessage(new SeasonProcessNotification(
                        IdHelpers.GetUniqueId(),
                        TargetAudience.All,
                        NotificationType.Update,
                        result.Season,
                        SeriesType.Movie,
                        $"Season information for {result.Season.Season}-{result.Season.Year} has been updated recently"),
                    new MinutesDelay(1));
                
                
                return new ScrapMoviesLibraryOutput
                {
                    AnimeMessages = result.Movies.Select(Serializer.ToJson),
                    ImagesMessages = result.Images.Select(Serializer.ToJson)
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
                    SeriesType.Ova,
                    "An error occurred before storing Movies."));

                return new ScrapMoviesLibraryOutput
                {
                    AnimeMessages = null,
                    ImagesMessages = null
                };
            });
    }

    private Task<Either<DomainError, MoviesLibraryForStorage>> RunCommand(MoviesUpdate command)
    {
        return command.Type switch
        {
            ShortSeriesUpdateType.Latest => _mediator.Send(new GetMoviesLibraryQry()),
            ShortSeriesUpdateType.Season => _mediator.Send(new GetScrappedMoviesLibraryQry(command.SeasonInformation!)),
            _ => throw new ArgumentOutOfRangeException(nameof(command.Type), "Movie update type has is invalid")
        };
    }
}