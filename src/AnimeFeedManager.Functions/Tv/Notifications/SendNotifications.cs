using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Infrastructure.SendGrid;
using AnimeFeedManager.Features.Notifications.IO;
using AnimeFeedManager.Features.Tv.Feed;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Notifications;

public class SendNotifications(
    EmailClient client,
    EmailConfiguration emailConfiguration,
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
            var emailContent = new EmailContent(EmailMessageExtensions.DefaultSubject())
            {
                Html = EmailMessageExtensions.CreateHtmlBody(notification.Feeds),
            };
            
            var emailMessage = new EmailMessage(emailConfiguration.FromEmail, notification.Subscriber, emailContent);
            
            var emailOperation = await client.SendAsync(WaitUntil.Completed, emailMessage, token);

            var response = await emailOperation.WaitForCompletionAsync(token);
            if (response.HasValue && response.Value.Status == EmailSendStatus.Succeeded)
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
            {
                _logger.LogError("Error sending email notification (Status {Status})", response.HasValue ? response.Value.Status: "Unknown");
            }
           
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