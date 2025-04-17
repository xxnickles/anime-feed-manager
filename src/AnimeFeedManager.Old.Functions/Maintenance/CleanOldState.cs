using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Features.Maintenance.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Maintenance;

public class CleanOldState(IStorageCleanup storageCleanup, ILoggerFactory loggerFactory)
{
    private readonly ILogger<CleanOldState> _logger = loggerFactory.CreateLogger<CleanOldState>();

    [Function(nameof(CleanOldState))]
    public async Task RunCleanState([TimerTrigger("0 0 10 * * SAT")] TimerInfo timer,
        CancellationToken token)
    {
        var notificationTypes = new[]
        {
            NotificationTarget.Admin,
            NotificationTarget.Images,
            NotificationTarget.Movie,
            NotificationTarget.Ova,
            NotificationTarget.Tv
        };

        foreach (var type in notificationTypes)
        {
            var result = await storageCleanup.CleanOldState(type, DateTime.Now.AddDays(-7), default);
            result.Match(
                _ => { _logger.LogInformation("Old state has been cleaned"); },
                e => e.LogError(_logger)
            );
        }
    }
}