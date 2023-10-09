using System.Collections.Immutable;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Types;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using AnimeFeedManager.Features.Users.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Notifications
{
    public class EnqueueNotifications
    {
        private readonly UserNotificationsCollector _notificationsCollector;
        private readonly IUserGetter _userGetter;


        private readonly ILogger<EnqueueNotifications> _logger;

        public EnqueueNotifications(
            UserNotificationsCollector notificationsCollector,
            IUserGetter userGetter,
            ILoggerFactory loggerFactory)
        {
            _notificationsCollector = notificationsCollector;
            _userGetter = userGetter;
            _logger = loggerFactory.CreateLogger<EnqueueNotifications>();
        }

        [Function("EnqueueNotifications")]
        public async Task Run(
            [TimerTrigger("0 0 * * * *")] TimerInfo timer
            // [TimerTrigger("0 0/1 * * * *")] TimerInfo timer
        )
        {
            var users = await _userGetter.GetAvailableUsers(default);

            await users.Match(
                ProcessUsers,
                error =>
                {
                    error.LogDomainError(_logger);
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
                    error => error.LogDomainError(_logger)
                );
            }
        }

        private Task<Either<DomainError, CollectedNotificationResult>> Process(string userId)
        {
            return UserIdValidator.Validate(userId)
                .ValidationToEither()
                .BindAsync(user => _notificationsCollector.Get(user));
        }
    }
}