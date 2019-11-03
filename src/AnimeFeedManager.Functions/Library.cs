using System.Threading.Tasks;
using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions
{
    public class Library
    {
        private readonly IMediator _mediator;

        public Library(IMediator mediator) => _mediator = mediator;

        [FunctionName("GetSeasonLibrary")]
        public Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "library/{season}")]
            HttpRequest req,
            string season,
            ILogger log)
        {
            return _mediator.Send(new GetSeasonCollection(season))
                .ToActionResult(log);
        }
    }
}