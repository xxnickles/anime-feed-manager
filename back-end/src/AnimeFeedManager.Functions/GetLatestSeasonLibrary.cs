using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions
{
    public class GetLatestSeasonLibrary
    {
        private readonly IMediator _mediator;

        public GetLatestSeasonLibrary(IMediator mediator) => _mediator = mediator;

        [FunctionName("GetLatestSeasonLibrary")]
        public Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "library/latest")] HttpRequest req,
            ILogger log)
        {
            return _mediator.Send(new GetLatestSeasonCollection())
                .ToActionResult(log);
        }
    }
}
