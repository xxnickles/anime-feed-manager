using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AnimeFeedManager.Functions.Features.Library;

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
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "seasons")] HttpRequestData req)
    {
        return _mediator.Send(new Application.Seasons.Queries.GetAvailableSeasons())
            .MapAsync(Map)
            .ToResponse(req, _logger);
    }

    private static ImmutableList<SeasonInfo> Map(ImmutableList<SeasonInformation> source)
    {
        return source.ConvertAll(x => new SeasonInfo
        {
            Season = x.Season.Value,
            Year = OptionUtils.UnpackOption<ushort>(x.Year.Value, 0)
        });
    }
}