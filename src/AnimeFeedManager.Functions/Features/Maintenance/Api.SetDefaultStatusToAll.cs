using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using MediatR;
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
                v => { return v.Select(SetDefaultStatus).Select(Serializer.ToJson); },
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