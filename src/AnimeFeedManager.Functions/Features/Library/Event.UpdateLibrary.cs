using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class GetLibraryMessages
{
    [QueueOutput(QueueNames.AnimeLibrary)] public IEnumerable<string>? AnimeMessages { get; set; }

    [QueueOutput(QueueNames.AvailableSeasons, Connection = "AzureWebJobsStorage")]
    public string? SeasonMessage { get; set; }
}

public class UpdateLibrary
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetLibraryMessages> _logger;

    public UpdateLibrary(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetLibraryMessages>();
    }

    [Function("GetLibrary")]
    public async Task<GetLibraryMessages> Run(
        [TimerTrigger("0 0 2 * * SAT")] TimerInfo timer
    )
    {
        var result = await _mediator.Send(new GetExternalLibraryQry());
        return result.Match(
            v =>
            {
                var seasonInfo = ExtractSeasonInformation(v.First());
                return new GetLibraryMessages
                {
                    AnimeMessages = v.Select(Serializer.ToJson),
                    SeasonMessage = Serializer.ToJson(seasonInfo)
                };
            },
            e =>
            {
                _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message);
                return new GetLibraryMessages();
            });
    }

    private static SeasonInfoDto ExtractSeasonInformation(AnimeInfoStorage sample) => new(sample?.Season ?? string.Empty, sample?.Year ?? 0);
}