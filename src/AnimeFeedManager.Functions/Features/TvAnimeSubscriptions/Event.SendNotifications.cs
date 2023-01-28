using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Infrastructure;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Interface;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AnimeFeedManager.Functions.Features.TvAnimeSubscriptions;

public class SendNotifications
{
    private readonly ISendGridClient _client;
    private readonly ISendGridConfiguration _sendGridConfiguration;
    private readonly INotificationsRepository _notificationsRepository;
    private readonly ILogger<SendNotifications> _logger;

    public SendNotifications(
        ISendGridClient client,
        ISendGridConfiguration sendGridConfiguration,
        INotificationsRepository notificationsRepository,
        ILoggerFactory loggerFactory)
    {
        _client = client;
        _sendGridConfiguration = sendGridConfiguration;
        _notificationsRepository = notificationsRepository;
        _logger = loggerFactory.CreateLogger<SendNotifications>();
    }

    [Function("SendNotifications")]
    public async Task Run(
        [QueueTrigger(QueueNames.Notifications, Connection = "AzureWebJobsStorage")]
        Notification notification)
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
                await _notificationsRepository.Merge(
                    Guid.NewGuid().ToString(), 
                    notification.Subscriber,
                    NotificationFor.Tv, 
                    NotificationType.Feed, 
                    new TvNotification(DateTime.Now, notification.Feeds));
                _logger.LogInformation("Sending notification to {NotificationSubscriber}", notification.Subscriber);
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