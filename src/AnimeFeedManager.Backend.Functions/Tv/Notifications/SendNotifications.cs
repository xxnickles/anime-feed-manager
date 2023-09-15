using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Notifications.IO;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using AnimeFeedManager.Features.Infrastructure.SendGrid;

namespace AnimeFeedManager.Backend.Functions.Tv.Notifications;

public class SendNotifications
{
    private readonly ISendGridClient _client;
    private readonly SendGridConfiguration _sendGridConfiguration;
    private readonly IStoreNotification _storeNotification;
    private readonly ILogger<SendNotifications> _logger;

    public SendNotifications(
        ISendGridClient client,
        SendGridConfiguration sendGridConfiguration,
        IStoreNotification storeNotification,
        ILoggerFactory loggerFactory)
    {
        _client = client;
        _sendGridConfiguration = sendGridConfiguration;
        _storeNotification = storeNotification;
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
                    default);

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
}