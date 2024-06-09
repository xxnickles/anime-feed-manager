using System.Collections.Immutable;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using AnimeFeedManager.Features.Users.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Notifications;

public class EnqueueNotifications(
    UserNotificationsCollector notificationsCollector,
    IUserGetter userGetter,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<EnqueueNotifications> _logger = loggerFactory.CreateLogger<EnqueueNotifications>();

    [Function("EnqueueNotifications")]
    public async Task Run(
        [TimerTrigger("0 0 * * * *")] TimerInfo timer,
        // [TimerTrigger("0 0/1 * * * *")] TimerInfo timer
        CancellationToken token
    )
    {
        var users = await userGetter.GetAvailableUsers(token);

        await users.Match(
            ProcessUsers,
            error =>
            {
                error.LogError(_logger);
                return Task.CompletedTask;
            });
    }

    private async Task ProcessUsers(ImmutableList<string> users)
    {
        var tasks = users.ConvertAll(Process);
        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            result.Match(
                notificationResult =>
                    _logger.LogInformation("A notification with {Count} series will be sent to {UserId}",
                        notificationResult.SeriesCount.ToString(), notificationResult.Subscriber),
                error => error.LogError(_logger)
            );
        }
    }

    private Task<Either<DomainError, CollectedNotificationResult>> Process(string userId)
    {
        return UserId.Parse(userId)
            .BindAsync(user => notificationsCollector.Get(user));
    }
}