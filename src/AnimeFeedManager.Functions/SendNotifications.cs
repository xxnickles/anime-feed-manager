using AnimeFeedManager.Application.Notifications;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace AnimeFeedManager.Functions
{
    public class SendNotifications
    {
        private readonly ISendGridConfiguration _sendGridConfiguration;

        public SendNotifications(ISendGridConfiguration sendGridConfiguration)
        {
            _sendGridConfiguration = sendGridConfiguration;
        }

        [FunctionName("SendNotifications")]
        public void Run(
            [QueueTrigger("notifications", Connection = "AzureWebJobsStorage")]Notification notification,
            [SendGrid(ApiKey = "SendGridKey")] out SendGridMessage message,
            ILogger log)
        {
            message = new SendGridMessage();
            message.SetFrom(new EmailAddress(_sendGridConfiguration.FromEmail, _sendGridConfiguration.FromName));
            message.SetSandBoxMode(_sendGridConfiguration.Sandbox);
            message.AddInfoFromNotification(notification);
            log.LogInformation($"Sending notification to {notification.Subscriber}");
        }
    }
}
