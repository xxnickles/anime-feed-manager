using AnimeFeedManager.Application.TvAnimeLibrary.Queries;
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "management/set-status")]
        HttpRequestData req)
    {

        var result = await req.AllowAdminOnly(new GetAllQry())
            .BindAsync(r => _mediator.Send(r));


        return await result.MatchAsync(
            async r => new SetDefaultStatusResponse
            {
                AnimeMessages = r.Select(SetDefaultStatus).Select(Serializer.ToJson),
                HttpResponse = await req.Ok()
            },
            async e => new SetDefaultStatusResponse
            {
                AnimeMessages = Enumerable.Empty<string>(),
                HttpResponse = await e.ToResponse(req, _logger)
            }
        );
    }

    private static AnimeInfoStorage SetDefaultStatus(AnimeInfoStorage storage)
    {
        storage.Completed = false;
        return storage;
    }
}