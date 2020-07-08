using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions
{
    public class SetDefaultStatusToAll
    {
        private readonly IMediator _mediator;

        public SetDefaultStatusToAll(IMediator mediator) => _mediator = mediator;

        [FunctionName("SetDefaultStatusToAll")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            [Queue("anime-library")] IAsyncCollector<AnimeInfoStorage> animeQueueCollector,
            ILogger log)
        {
            var result = await _mediator.Send(new GetAll());
            result.Match(
                v =>
                {
                    var mapped = v.Select(SetDefaultStatus).ToImmutableList();
                    QueueStorage.StoreInQueue(mapped, animeQueueCollector, log, x => $"Queueing for update {x.Title}");
                },
                e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
            );
        }

        private static AnimeInfoStorage SetDefaultStatus(AnimeInfoStorage storage)
        {
            storage.Completed = false;
            return storage;
        }
    }
}
