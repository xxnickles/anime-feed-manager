using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Infrastructure.SendGrid;
using AnimeFeedManager.Features.Notifications.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AnimeFeedManager.Functions.Tv.Notifications;

public class SendNotifications
{
    private readonly ISendGridClient _client;
    private readonly SendGridConfiguration _sendGridConfiguration;
    private readonly IStoreNotification _storeNotification;
    private readonly IAddProcessedTitles _addProcessedTitles;
    private readonly ILogger<SendNotifications> _logger;

    public SendNotifications(
        ISendGridClient client,
        SendGridConfiguration sendGridConfiguration,
        IStoreNotification storeNotification,
        IAddProcessedTitles addProcessedTitles,
        ILoggerFactory loggerFactory)
    {
        _client = client;
        _sendGridConfiguration = sendGridConfiguration;
        _storeNotification = storeNotification;
        _addProcessedTitles = addProcessedTitles;
        _logger = loggerFactory.CreateLogger<SendNotifications>();
    }

    [Function("SendNotifications")]
    public async Task Run(
        [QueueTrigger(Box.Available.TvNotificationsBox, Connection = "AzureWebJobsStorage")]
        SubscriberTvNotification notification)
    {
        try
        {
            var message = new SendGridMessage();
            message.SetFrom(new EmailAddress(_sendGridConfiguration.FromEmail, _sendGridConfiguration.FromName));
            message.SetSandBoxMode(_sendGridConfiguration.Sandbox);
            message.AddInfoFromNotification(notification);
            var response = await _client.SendEmailAsync(message);
            if (response.IsSuccessStatusCode)
            {
                var result = await _storeNotification.Add(
                        Guid.NewGuid().ToString(),
                        notification.Subscriber,
                        NotificationTarget.Tv,
                        NotificationArea.Feed,
                        new TvFeedUpdateNotification(TargetAudience.User, NotificationType.Update,
                            "Notification has been sent", DateTime.Now, notification.Feeds),
                        default)
                    .BindAsync(_ => _addProcessedTitles.Add(GetTitlesForUser(notification), default));

                result.Match(
                    _ => _logger.LogInformation("Sending notification to {NotificationSubscriber}",
                        notification.Subscriber),
                    error => error.LogDomainError(_logger)
                );
            }
            else
                _logger.LogError("Error sending email notification (Status Code {Code}) {Reason}", response.StatusCode,
                    await response.Body.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Message email sent has failed for {User}", notification.Subscriber);
        }
    }

    private IEnumerable<(string User, string Title)> GetTitlesForUser(SubscriberTvNotification notification)
    {
        return notification.Feeds.Select(feed => (notification.SubscriberId, feed.Title));
    }
}