using System.Collections.Immutable;
using AnimeFeedManager.Application.TvAnimeLibrary.Commands;
using AnimeFeedManager.Application.TvAnimeLibrary.Queries;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Common.Notifications.Realtime;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Services.Collectors.Interface;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnime;

public class ScrapTvLibraryOutput
{
    [QueueOutput(QueueNames.TitleProcess)] 
    public string? TitleMessage { get; set; }

    [QueueOutput(QueueNames.TvAnimeLibraryUpdates)]
    public IEnumerable<string>? AnimeMessages { get; set; }

    [QueueOutput(QueueNames.ImageProcess)] 
    public IEnumerable<string>? ImagesMessages { get; set; }

    [QueueOutput(QueueNames.AvailableSeasonsUpdates)]
    public string? SeasonMessage { get; set; }
}

public class ScrapTvLibrary
{
    private readonly record struct StateLibraryForStorage(
        ImmutableList<StateWrapper<AnimeInfoStorage>> Animes,
        ImmutableList<StateWrapper<BlobImageInfoEvent>> Images,
        SeasonInfoDto Season
    );

    private readonly IUpdateState _updateState;
    private readonly IDomainPostman _domainPostman;
    private readonly IFeedProvider _feedProvider;
    private readonly IMediator _mediator;
    private readonly ILogger<ScrapTvLibrary> _logger;

    public ScrapTvLibrary(
        IUpdateState updateState,
        IDomainPostman domainPostman,
        IFeedProvider feedProvider,
        IMediator mediator,
        ILoggerFactory loggerFactory)
    {
        _updateState = updateState;
        _domainPostman = domainPostman;
        _feedProvider = feedProvider;
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<ScrapTvLibrary>();
    }

    [Function("ScrapTvLibrary")]
    public async Task<ScrapTvLibraryOutput> Run(
        [QueueTrigger(QueueNames.TvAnimeLibraryUpdate, Connection = "AzureWebJobsStorage")]
        LibraryUpdate startProcess)
    {
        return startProcess.Type switch
        {
            TvUpdateType.Full => await ProcessFullLibrary(),
            TvUpdateType.Titles => await ProcessTitles(),
            _ => new ScrapTvLibraryOutput()
        };
    }


    private async Task<ScrapTvLibraryOutput> ProcessTitles()
    {
        _logger.LogInformation("Processing update of the feed titles only");
        var titleStoreResult = await _feedProvider.GetTitles()
            .BindAsync(feedTitles => _mediator.Send(new AddTitlesCmd(feedTitles)));

        return titleStoreResult.Match(_ =>
        {
            _domainPostman.SendMessage(new TitlesUpdateNotification(
                IdHelpers.GetUniqueId(),
                TargetAudience.Admins,
                NotificationType.Information,
                "Latest feed titles have been updated"));

            return new ScrapTvLibraryOutput
            {
                TitleMessage = ProcessResult.Ok
            };
        }, e =>
        {
            _logger.LogError("An error occurred while processing feed titles update {S}", e.ToString());
            _domainPostman.SendMessage(new TitlesUpdateNotification(
                IdHelpers.GetUniqueId(),
                TargetAudience.Admins,
                NotificationType.Error,
                "An error occurred before storing feed titles."));

            return new ScrapTvLibraryOutput
            {
                AnimeMessages = null,
                ImagesMessages = null,
                TitleMessage = ProcessResult.Failure,
                SeasonMessage = null
            };
        });
    }

    private async Task<ScrapTvLibraryOutput> ProcessFullLibrary()
    {
        _logger.LogInformation("Processing update of the full library");

        var result = await _feedProvider.GetTitles()
            .BindAsync(CollectLibrary)
            .BindAsync(AddState);


        return result.Match(
            v =>
            {
                _logger.LogInformation("Titles have been updated and Series information has been collected");

                return new ScrapTvLibraryOutput
                {
                    AnimeMessages = v.Animes.Select(Serializer.ToJson),
                    ImagesMessages = v.Images.Select(Serializer.ToJson),
                    SeasonMessage = Serializer.ToJson(v.Season),
                    TitleMessage = ProcessResult.Ok
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
                    "An error occurred before storing series."));

                return new ScrapTvLibraryOutput
                {
                    AnimeMessages = null,
                    ImagesMessages = null,
                    TitleMessage = ProcessResult.Failure,
                    SeasonMessage = null
                };
            });
    }

    private Task<Either<DomainError, LibraryForStorage>> CollectLibrary(ImmutableList<string> feedTitles)
    {
        return _mediator.Send(new AddTitlesCmd(feedTitles))
            .BindAsync(_ => _mediator.Send(new GetScrappedLibraryQry(feedTitles)));
    }


    private Task<Either<DomainError, StateLibraryForStorage>> AddState(LibraryForStorage library)
    {
        Task<Either<DomainError, (string seriesStateId, string imagesStateId)>> CombineIds(string seriesStateId)
        {
            return _updateState.Create(Common.Notifications.NotificationType.Images, library.Images.Count)
                .MapAsync(imgIds => (seriesStateId, imgIds));
        }

        return _updateState.Create(Common.Notifications.NotificationType.Tv, library.Animes.Count)
            .BindAsync(CombineIds)
            .MapAsync(r => new StateLibraryForStorage(
                library.Animes.ConvertAll(a => new StateWrapper<AnimeInfoStorage>(r.seriesStateId, a)),
                library.Images.ConvertAll(i => new StateWrapper<BlobImageInfoEvent>(r.imagesStateId, i)),
                library.Season
            ));
    }
}