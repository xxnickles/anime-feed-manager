using System.Security.Claims;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.RealTimeNotifications;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using HubInfoContext = AnimeFeedManager.Common.RealTimeNotifications.HubInfoContext;

namespace AnimeFeedManager.Functions.RealTime;

public class GroupDataOutput
{
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public SignalRGroupAction? GroupOutput { get; set; }

    public HttpResponseData? HttpResponse { get; set; }
}

public class SetupGroupData(ILoggerFactory loggerFactory)
{
    private readonly ILogger<SetupGroupData> _logger = loggerFactory.CreateLogger<SetupGroupData>();

    [Function("SetupGroupData")]
    public async Task<GroupDataOutput> Add(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notifications/setup")]
        HttpRequestData req)
    {
        _logger.LogInformation("Setting up signalr group data");
        var authResult = await req.CheckAuthorization();

        return await authResult.Match(
            OkResponse,
            e => ErrorResponse(req, e)
        );
    }

    private async Task<GroupDataOutput> OkResponse(
        (ClaimsPrincipal principal, HttpRequestData request) parameters)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(parameters.request.Body, HubInfoContext.Default.HubInfo);

        ArgumentNullException.ThrowIfNull(payload);

        var groupAction = parameters.principal.IsInRole(RoleNames.Admin)
            ? new SignalRGroupAction(SignalRGroupActionType.Add)
            {
                UserId = parameters.principal.Identity?.Name ?? string.Empty,
                GroupName = HubGroups.AdminGroup,
                ConnectionId = payload.ConnectionId
            }
            : new SignalRGroupAction(SignalRGroupActionType.Add)
            {
                UserId = parameters.principal.Identity?.Name ?? string.Empty,
                GroupName = HubGroups.UserGroup,
                ConnectionId = payload.ConnectionId
            };
        _logger.LogInformation("Adding {Connection} {Predicate}. User Id {User}",
            payload.ConnectionId,
            string.IsNullOrEmpty(groupAction.GroupName) ? " and user information to hub" : " to admin group",
            groupAction.UserId);
        return new GroupDataOutput
        {
            GroupOutput = groupAction,
            HttpResponse = await parameters.request.Ok()
        };
    }

    private async Task<GroupDataOutput> ErrorResponse(HttpRequestData req, DomainError error)
    {
        return new GroupDataOutput
        {
            HttpResponse = await error.ToResponse(req, _logger)
        };
    }
}