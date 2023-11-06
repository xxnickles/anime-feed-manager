using AnimeFeedManager.Common.RealTimeNotifications;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using HubInfoContext = AnimeFeedManager.Common.RealTimeNotifications.HubInfoContext;

namespace AnimeFeedManager.Functions.RealTime;

public class RemoveDataOutput
{
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public SignalRGroupAction? GroupOutput { get; set; }

    public HttpResponseData? HttpResponse { get; set; }
}

public class RemoveGroupData(ILoggerFactory loggerFactory)
{
    private readonly ILogger<RemoveGroupData> _logger = loggerFactory.CreateLogger<RemoveGroupData>();

    [Function("RemoveGroupData")]
    public async Task<RemoveDataOutput> Add(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notifications/remove")]
        HttpRequestData req)
    {
        var payload =
            await JsonSerializer.DeserializeAsync(req.Body, HubInfoContext.Default.HubInfo);

        ArgumentNullException.ThrowIfNull(payload);
        _logger.LogInformation("Removing {Connection} from group", payload.ConnectionId);
        var groupAction = new SignalRGroupAction(SignalRGroupActionType.Remove)
        {
            GroupName = HubGroups.AdminGroup,
            ConnectionId = payload.ConnectionId
        };

        return new RemoveDataOutput
        {
            GroupOutput = groupAction,
            HttpResponse = await req.Ok()
        };
    }
}