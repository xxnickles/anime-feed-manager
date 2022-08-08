using System.Collections.Immutable;
using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Application.Feed.Commands;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Services.Collectors.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Scrapping;

public class ProcessLibraryOutput
{
    [QueueOutput(QueueNames.TitleProcess)] public string? TitleMessage { get; set; }

    [QueueOutput(QueueNames.AnimeLibrary)] public IEnumerable<string>? AnimeMessages { get; set; }

    [QueueOutput(QueueNames.ImageProcess)] public IEnumerable<string>? ImagesMessages { get; set; }

    [QueueOutput(QueueNames.AvailableSeasons)]
    public string? SeasonMessage { get; set; }
}

public class ProcessLibrary
{
    private readonly IFeedProvider _feedProvider;
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessLibrary> _logger;

    public ProcessLibrary(
        IFeedProvider feedProvider,
        IMediator mediator,
        ILoggerFactory loggerFactory)
    {
        _feedProvider = feedProvider;
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<ProcessLibrary>();
    }

    [Function("ProcessLibrary")]
    public async Task<ProcessLibraryOutput> Run(
        [QueueTrigger(QueueNames.LibraryUpdate, Connection = "AzureWebJobsStorage")]
        string startProcess)
    {
        var result = await _feedProvider.GetTitles()
            .BindAsync(CollectLibrary);


        return result.Match(
            v =>
            {
                _logger.LogInformation("Titles have been updated and Series information has been collected");
                return new ProcessLibraryOutput
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
                return new ProcessLibraryOutput
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
            .BindAsync(_ => _mediator.Send(new GetLibraryQry(feedTitles)));
    }
}