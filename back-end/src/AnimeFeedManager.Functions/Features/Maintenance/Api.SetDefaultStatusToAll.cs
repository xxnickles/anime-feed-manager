using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Maintenance;

public class SetDefaultStatusResponse
{
    [QueueOutput(QueueNames.AnimeLibrary)] public IEnumerable<string>? AnimeMessages { get; set; }

    public HttpResponseData? HttpResponse { get; set; }
}

public class SetDefaultStatusToAll
{
    private readonly IMediator _mediator;
    private readonly ILogger<SetDefaultStatusToAll> _logger;

    public SetDefaultStatusToAll(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<SetDefaultStatusToAll>();
    }

    [Function("SetDefaultStatusToAll")]
    public async Task<SetDefaultStatusResponse> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)]
        HttpRequestData req)
    {
        var result = await _mediator.Send(new GetAllQry());


        return new SetDefaultStatusResponse
        {
            AnimeMessages = result.Match(
                v => { return v.Select(SetDefaultStatus).Select(x => JsonSerializer.Serialize(x)); },
                e =>
                {
                    _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message);
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