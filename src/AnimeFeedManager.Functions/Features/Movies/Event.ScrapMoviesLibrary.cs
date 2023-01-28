using System.Collections.Immutable;
using AnimeFeedManager.Application.MoviesLibrary.Queries;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications.Realtime;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
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
    private readonly record struct StateLibraryForStorage(
        ImmutableList<StateWrapper<MovieStorage>> Movies,
        ImmutableList<StateWrapper<BlobImageInfoEvent>> Images,
        SeasonInfoDto Season
    );

    private readonly IUpdateState _updateState;
    private readonly IDomainPostman _domainPostman;
    private readonly IMediator _mediator;
    private readonly ILogger<ScrapMoviesLibrary> _logger;

    public ScrapMoviesLibrary(
        IUpdateState updateState,
        IDomainPostman domainPostman,
        IMediator mediator,
        ILoggerFactory loggerFactory)
    {
        _updateState = updateState;
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

        var result = await RunCommand(payload).BindAsync(AddState);

        return result.Match(
            result => new ScrapMoviesLibraryOutput
            {
                AnimeMessages = result.Movies.Select(Serializer.ToJson),
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
                    SeriesType.Movie,
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

    private Task<Either<DomainError, StateLibraryForStorage>> AddState(MoviesLibraryForStorage library)
    {
        Task<Either<DomainError, (string seriesStateId, string imagesStateId)>> CombineIds(string seriesStateId)
        {
            return _updateState.Create(Common.Notifications.NotificationFor.Images, library.Images.Count)
                .MapAsync(imgIds => (seriesStateId, imgIds));
        }

        return _updateState.Create(Common.Notifications.NotificationFor.Movie, library.Movies.Count)
            .BindAsync(CombineIds)
            .MapAsync(r => new StateLibraryForStorage(
                library.Movies.ConvertAll(a => new StateWrapper<MovieStorage>(r.seriesStateId, a)),
                library.Images.ConvertAll(i => new StateWrapper<BlobImageInfoEvent>(r.imagesStateId, i)),
                library.Season
            ));
    }
}