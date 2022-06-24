using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions.Features.Library;

public class GetLatestSeasonLibrary
{
    private readonly IMediator _mediator;

    public GetLatestSeasonLibrary(IMediator mediator) => _mediator = mediator;

    [FunctionName("GetLatestSeasonLibrary")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "library/latest")] HttpRequestData req,
        ILogger log)
    {
        return _mediator.Send(new GetLatestSeasonCollection())
            .ToResponse(req,log);
    }
}