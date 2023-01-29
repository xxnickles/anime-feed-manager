using System.Collections.Immutable;
using AnimeFeedManager.Application.TvAnimeLibrary.Queries;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Maintenance;

public class SetDefaultStatusResponse
{
    [QueueOutput(QueueNames.TvAnimeLibraryUpdates)] public IEnumerable<string>? AnimeMessages { get; set; }

    public HttpResponseData? HttpResponse { get; set; }
}

public class SetDefaultStatusToAll
{
    private readonly IUpdateState _updateState;
    private readonly IMediator _mediator;
    private readonly ILogger<SetDefaultStatusToAll> _logger;


    public SetDefaultStatusToAll(
        IUpdateState updateState,
        IMediator mediator, 
        ILoggerFactory loggerFactory)
    {
        _updateState = updateState;
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<SetDefaultStatusToAll>();
    }

    [Function("SetDefaultStatusToAll")]
    public async Task<SetDefaultStatusResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "management/set-status")]
        HttpRequestData req)
    {

        var result = await req.AllowAdminOnly(new GetAllQry())
            .BindAsync(r => _mediator.Send(r))
            .MapAsync(r => r.ConvertAll(SetDefaultStatus))
            .BindAsync(AddState);


        return await result.MatchAsync(
            async r => new SetDefaultStatusResponse
            {
                AnimeMessages = r.Select(Serializer.ToJson),
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
    
    private Task<Either<DomainError, ImmutableList<StateWrapper<AnimeInfoStorage>>>> AddState(
        ImmutableList<AnimeInfoStorage> animes)
    {
        return _updateState.Create(NotificationFor.Tv, animes.Count)
            .MapAsync(id => animes.ConvertAll(a => new StateWrapper<AnimeInfoStorage>(id, a)));
    }
}