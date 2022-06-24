using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AnimeFeedManager.Functions.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AnimeFeedManager.Functions.Features.Maintenance;

public class SetDefaultStatusToAll
{
    private readonly IMediator _mediator;

    public SetDefaultStatusToAll(IMediator mediator) => _mediator = mediator;

    [FunctionName("SetDefaultStatusToAll")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequestData req,
        [QueueTrigger(QueueNames.AnimeLibrary)] IAsyncCollector<AnimeInfoStorage> animeQueueCollector,
        ILogger log)
    {
        var result = await _mediator.Send(new GetAll());
        result.Match(
            v =>
            {
                var mapped = v.Select(SetDefaultStatus).ToImmutableList();
                QueueStorage.StoreInQueue(mapped, animeQueueCollector, log, x => $"Queueing for update {x.Title}");
            },
            e => log.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );

        return await req.Ok();
    }

    private static AnimeInfoStorage SetDefaultStatus(AnimeInfoStorage storage)
    {
        storage.Completed = false;
        return storage;
    }
}