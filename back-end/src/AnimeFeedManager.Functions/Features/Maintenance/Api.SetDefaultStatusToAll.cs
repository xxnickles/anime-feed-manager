using System.Collections.Generic;
using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AnimeFeedManager.Functions.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AnimeFeedManager.Functions.Features.Maintenance;

public class SetDefaultStatusResponse
{
    [QueueOutput(QueueNames.AnimeLibrary)] public IEnumerable<string>? AnimeMessages { get; set; }

    public HttpResponseData? HttpResponse { get; set; }
}

public class SetDefaultStatusToAll
{
    private readonly IMediator _mediator;

    public SetDefaultStatusToAll(IMediator mediator) => _mediator = mediator;

    [FunctionName("SetDefaultStatusToAll")]
    public async Task<SetDefaultStatusResponse> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)]
        HttpRequestData req,
        ILogger log)
    {
        var result = await _mediator.Send(new GetAll());


        return new SetDefaultStatusResponse
        {
            AnimeMessages = result.Match(
                v => { return v.Select(SetDefaultStatus).Select(x => JsonSerializer.Serialize(x)); },
                e =>
                {
                    log.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message);
                    return null!;
                }),
            HttpResponse = await req.Ok()
        };
    }

    private static AnimeInfoStorage SetDefaultStatus(AnimeInfoStorage storage)
    {
        storage.Completed = false;
        return storage;
    }
}