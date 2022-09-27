using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Seasons;

public class GetAvailableSeasons
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetAvailableSeasons> _logger;

    public GetAvailableSeasons(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetAvailableSeasons>();
    }

    [Function("GetAvailableSeasons")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "seasons")]
        HttpRequestData req)
    {
        return _mediator.Send(new Application.Seasons.Queries.GetAvailableSeasonsQry())
            .MapAsync(Map)
            .ToResponse(req, _logger);
    }

    private static ImmutableList<SeasonInfoDto> Map(ImmutableList<SeasonInformation> source)
    {
        return source.ConvertAll(x =>
            new SeasonInfoDto(x.Season.Value, x.Year.Value.UnpackOption<ushort>(0)));
    }
}