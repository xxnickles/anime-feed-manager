using AnimeFeedManager.Application.Notifications;
using AnimeFeedManager.Functions.Infrastructure;
using AnimeFeedManager.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class SendNotifications
{
    private readonly ISendGridConfiguration _sendGridConfiguration;
    private readonly ILogger<SendNotifications> _logger;

    public SendNotifications(ISendGridConfiguration sendGridConfiguration, ILoggerFactory loggerFactory)
    {
        _sendGridConfiguration = sendGridConfiguration;
        _logger = loggerFactory.CreateLogger<SendNotifications>();
    }

    [Function("SendNotifications")]
    public void Run(
        [QueueTrigger(QueueNames.Notifications, Connection = "AzureWebJobsStorage")]
        Notification notification,
        [SendGrid(ApiKey = "SendGridKey")] out SendGridMessage message)
    {
        message = new SendGridMessage();
        message.SetFrom(new EmailAddress(_sendGridConfiguration.FromEmail, _sendGridConfiguration.FromName));
        message.SetSandBoxMode(_sendGridConfiguration.Sandbox);
        message.AddInfoFromNotification(notification);
        _logger.LogInformation("Sending notification to {NotificationSubscriber}", notification.Subscriber);
    }
}