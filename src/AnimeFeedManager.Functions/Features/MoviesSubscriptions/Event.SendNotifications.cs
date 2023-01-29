using AnimeFeedManager.Application;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Infrastructure;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Interface;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AnimeFeedManager.Functions.Features.MoviesSubscriptions;

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

    [Function("SendMoviesNotifications")]
    public async Task<IEnumerable<ShortSeriesUpdate>> Run(
        [QueueTrigger(QueueNames.MoviesMarkCompletedProcess, Connection = "AzureWebJobsStorage")]
        ShortSeriesSubscriptionCollection notification)
    {
        try
        {
            var message = new SendGridMessage();
            message.SetFrom(new EmailAddress(_sendGridConfiguration.FromEmail, _sendGridConfiguration.FromName));
            message.SetSandBoxMode(_sendGridConfiguration.Sandbox);
            message.AddInfoFromNotification(notification, SeriesType.Movie.ToString());
            var response = await _client.SendEmailAsync(message);
            if (response.IsSuccessStatusCode)
            {
                await _notificationsRepository.Merge(
                    Guid.NewGuid().ToString(), 
                    notification.Subscriber,
                    NotificationFor.Movie, 
                    NotificationType.Feed, 
                    new ShortSeriesNotification(DateTime.Now, notification.Series));
                _logger.LogInformation("Sending Movies notification to {NotificationSubscriber}", notification.Subscriber);
                return notification.Series.Select(x => new ShortSeriesUpdate(notification.Subscriber, x.Title));
            }
            else
            {
                _logger.LogError("Error sending Movies email notification (Status Code {Code}) {Reason}", response.StatusCode,
                    await response.Body.ReadAsStringAsync());

                return Enumerable.Empty<ShortSeriesUpdate>();
            }
               
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Message email sent has failed for {User}", notification.Subscriber);
            return Enumerable.Empty<ShortSeriesUpdate>();
        }
    }
}