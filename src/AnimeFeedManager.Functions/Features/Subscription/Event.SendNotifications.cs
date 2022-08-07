using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Infrastructure;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class SendNotifications
{
    private readonly ISendGridClient _client;
    private readonly ISendGridConfiguration _sendGridConfiguration;
    private readonly ILogger<SendNotifications> _logger;

    public SendNotifications(ISendGridClient client, ISendGridConfiguration sendGridConfiguration,
        ILoggerFactory loggerFactory)
    {
        _client = client;
        _sendGridConfiguration = sendGridConfiguration;
        _logger = loggerFactory.CreateLogger<SendNotifications>();
    }

    [Function("SendNotifications")]
    public async Task Run(
        [QueueTrigger(QueueNames.Notifications, Connection = "AzureWebJobsStorage")]
        Notification notification)
    {
        var message = new SendGridMessage();
        message.SetFrom(new EmailAddress(_sendGridConfiguration.FromEmail, _sendGridConfiguration.FromName));
        message.SetSandBoxMode(_sendGridConfiguration.Sandbox);
        message.AddInfoFromNotification(notification);
        var response = await _client.SendEmailAsync(message);
        if (response.IsSuccessStatusCode)
            _logger.LogInformation("Sending notification to {NotificationSubscriber}", notification.Subscriber);
        else
            _logger.LogError("Error sending email notification (Status Code {Code}) {Reason}", response.StatusCode,
                await response.Body.ReadAsStringAsync());
    }
}