using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace AnimeFeedManager.Functions.Features.Library;

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
            Year = OptionUtils.UnpackOption<ushort>(x.Year.Value, 0)
        });
    }
}