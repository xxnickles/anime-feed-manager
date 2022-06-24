using System.Threading.Tasks;
using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class GetSeasonLibrary
{
    private readonly IMediator _mediator;

    public GetSeasonLibrary(IMediator mediator) => _mediator = mediator;

    [FunctionName("GetSeasonLibrary")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "library/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year,
        ILogger log)
    {
        return _mediator.Send(new GetSeasonCollection(season, year))
            .ToResponse(req,log);
    }
}