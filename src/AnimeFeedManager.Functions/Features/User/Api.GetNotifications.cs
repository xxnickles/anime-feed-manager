using AnimeFeedManager.Application.Notifications.Queries;
using AnimeFeedManager.Common;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.User;

public class GetNotifications
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetNotifications> _logger;

    public GetNotifications(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetNotifications>();
    }

    [Function("GetNotifications")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "notifications/{subscriber}")]
        HttpRequestData req,
        string subscriber
    )
    {
        return await req.CheckAuthorization()
            .BindAsync(r =>
            {
                var (principal, _) = r;
                return principal.IsInRole(UserRoles.Admin)
                    ? _mediator.Send(new GetAdminNotificationsQry(subscriber))
                    : _mediator.Send(new GetUserNotificationsQry(subscriber));
            })
            .ToResponse(req, _logger);
    }
}