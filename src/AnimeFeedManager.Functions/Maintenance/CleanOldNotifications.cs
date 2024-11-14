using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Maintenance.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Maintenance;

public class CleanOldNotifications(IStorageCleanup storageCleanup, ILoggerFactory loggerFactory)
{
    private readonly ILogger<CleanOldNotifications> _logger = loggerFactory.CreateLogger<CleanOldNotifications>();

    [Function(nameof(CleanOldNotifications))]
    public async Task RunCleanNotifications([TimerTrigger("0 0 0 1 * *")] TimerInfo timer, 
        CancellationToken token)
    {
        var result = await storageCleanup.CleanOldNotifications(DateTime.Now.AddDays(-30), token);
        result.Match(
            _ => { _logger.LogInformation("Old notifications have been cleaned"); },
            e => e.LogError(_logger)
        );
    }
}