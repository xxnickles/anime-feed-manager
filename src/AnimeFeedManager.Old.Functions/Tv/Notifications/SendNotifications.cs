using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Common.Dto;
using AnimeFeedManager.Old.Features.Infrastructure.SendGrid;
using AnimeFeedManager.Old.Features.Notifications.IO;
using AnimeFeedManager.Old.Features.Tv.Feed;
using AnimeFeedManager.Old.Features.Tv.Subscriptions.IO;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AnimeFeedManager.Old.Functions.Tv.Notifications;

public class SendNotifications(
    ISendGridClient client,
    SendGridConfiguration sendGridConfiguration,
    IStoreNotification storeNotification,
    IAddProcessedTitles addProcessedTitles,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<SendNotifications> _logger = loggerFactory.CreateLogger<SendNotifications>();

    [Function(nameof(SendNotifications))]
    public async Task Run(
        [QueueTrigger(SubscriberTvNotification.TargetQueue, Connection = Constants.AzureConnectionName)]
        SubscriberTvNotification notification, CancellationToken token)
    {
        try
        {
            var message = new SendGridMessage();
            message.SetFrom(new EmailAddress(sendGridConfiguration.FromEmail, sendGridConfiguration.FromName));
            message.SetSandBoxMode(sendGridConfiguration.Sandbox);
            message.AddInfoFromNotification(notification);
            var response = await client.SendEmailAsync(message, token);
            if (response.IsSuccessStatusCode)
            {
                var result = await storeNotification.Add(
                        Guid.NewGuid().ToString(),
                        notification.SubscriberId,
                        NotificationTarget.Tv,
                        NotificationArea.Feed,
                        new TvFeedUpdateNotification(TargetAudience.User, NotificationType.Update,
                            "Notification has been sent", DateTime.Now, notification.Feeds),
                        default)
                    .BindAsync(_ => addProcessedTitles.Add(GetTitlesForUser(notification), token));

                result.Match(
                    _ => _logger.LogInformation("Sending notification to {NotificationSubscriber}",
                        notification.SubscriberId),
                    error => error.LogError(_logger)
                );
            }
            else
                _logger.LogError("Error sending email notification (Status Code {Code}) {Reason}", response.StatusCode,
                    await response.Body.ReadAsStringAsync(token));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Message email sent has failed for {User}", notification.Subscriber);
        }
    }

    private static IEnumerable<(string User, string Title)> GetTitlesForUser(SubscriberTvNotification notification)
    {
        return notification.Feeds.Select(feed => (notification.SubscriberId, feed.Title));
    }
}