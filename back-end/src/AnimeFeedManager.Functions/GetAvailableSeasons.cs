using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions
{
    public class GetAvailableSeasons
    {
        private readonly IMediator _mediator;

        public GetAvailableSeasons(IMediator mediator) => _mediator = mediator;

        [FunctionName("GetAvailableSeasons")]
        public Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "seasons")] HttpRequest req,
            ILogger log)
        {
            return _mediator.Send(new Application.Seasons.Queries.GetAvailableSeasons())
                .MapAsync(Map)
                .ToActionResult(log);
        }

        private static ImmutableList<SeasonInfo> Map(ImmutableList<SeasonInformation> source)
        {
            return source.ConvertAll(x => new SeasonInfo
            {
                Season = x.Season.Value,
                Year = (int) OptionUtils.UnpackOption<ushort>(x.Year.Value, 0)
            });
        }
    }
}
