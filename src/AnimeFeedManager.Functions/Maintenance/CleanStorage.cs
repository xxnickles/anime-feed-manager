using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Maintenance.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Maintenance;

public class CleanStorage
{
    private readonly IStorageCleanup _storageCleanup;
    private readonly ILogger<CleanStorage> _logger;

    public CleanStorage(IStorageCleanup storageCleanup, ILoggerFactory loggerFactory)
    {
        _storageCleanup = storageCleanup;
        _logger = loggerFactory.CreateLogger<CleanStorage>();
    }

    [Function("CleanOldNotifications")]
    public async Task RunCleanNotifications([TimerTrigger("0 0 0 1 * *")] TimerInfo timer)
    {
        var result = await _storageCleanup.CleanOldNotifications(DateTime.Now.AddDays(-30), default);
        result.Match(
            _ => { _logger.LogInformation("Old notifications have been cleaned"); },
            e => e.LogDomainError(_logger)
        );
    }

    [Function("CleanOldState")]
    public async Task RunCleanState([TimerTrigger("0 0 10 * * SAT")] TimerInfo timer)
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
            var result = await _storageCleanup.CleanOldState(type, DateTime.Now.AddDays(-7), default);
            result.Match(
                _ => { _logger.LogInformation("Old state has been cleaned"); },
                e => e.LogDomainError(_logger)
            );
        }
    }
}