using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.RealTime;

public class RemoveDataOutput
{
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public SignalRGroupAction? GroupOutput { get; set; }

    public HttpResponseData? HttpResponse { get; set; }
}

public class RemoveGroupData
{
    private readonly ILogger<RemoveGroupData> _logger;

    public RemoveGroupData(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RemoveGroupData>();
    }

    [Function("RemoveGroupData")]
    public async Task<RemoveDataOutput> Add(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notifications/remove")]
        HttpRequestData req)
    {
      
        var dto = await Serializer.FromJson<HubInfo>(req.Body);
        ArgumentNullException.ThrowIfNull(dto);
        _logger.LogInformation("Removing {Connection} from group", dto.ConnectionId);
        var groupAction = new SignalRGroupAction(SignalRGroupActionType.Remove)
        {
            GroupName = HubGroups.AdminGroup,
            ConnectionId = dto.ConnectionId
        };

        return new RemoveDataOutput
        {
            GroupOutput = groupAction,
            HttpResponse = await req.Ok()
        };
    }
}