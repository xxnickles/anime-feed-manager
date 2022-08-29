using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.RealTime;

public class TestNotifications
{
    private readonly ILogger<TestNotifications> _logger;

    public TestNotifications(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TestNotifications>();
    }
    
    [Function("BroadcastToAll")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public static SignalRMessageAction BroadcastToAll(
        [TimerTrigger("0 * * * * *")] TimerInfo timer)
    {
      
        return new SignalRMessageAction(ServerNotifications.TestNotification)
        {
            // broadcast to all the connected clients without specifying any connection, user or group.
            Arguments = new[] { "This is a test" },
        };
    }
    
    [Function("BroadcastToAdmin")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public static SignalRMessageAction BroadcastToAdmin(
        [TimerTrigger("0 * * * * *")] TimerInfo timer)
    {
      
        return new SignalRMessageAction(ServerNotifications.TestNotification)
        {
            GroupName = HubGroups.AdminGroup,
            // broadcast to all the connected clients without specifying any connection, user or group.
            Arguments = new[] { "This is a test for Admin only" },
        };
    }
}