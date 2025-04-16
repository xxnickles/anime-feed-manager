using System.Collections.Immutable;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Infrastructure.SendGrid;
using AnimeFeedManager.Features.Notifications.IO;
using AnimeFeedManager.Features.Ovas.Subscriptions;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;
using AnimeFeedManager.Web.BlazorComponents.Templates;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AnimeFeedManager.Old.Functions.Ovas.Notifications;

public class CollectAndSendOvasNotification
{
    private readonly ISendGridClient _client;
    private readonly SendGridConfiguration _sendGridConfiguration;
    private readonly UserOvasFeedForProcess _userOvasFeedForProcess;
    private readonly IStoreNotification _storeNotification;
    private readonly BlazorRenderer _renderer;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<CollectAndSendOvasNotification> _logger;

    public CollectAndSendOvasNotification(
        ISendGridClient client,
        SendGridConfiguration sendGridConfiguration,
        UserOvasFeedForProcess userOvasFeedForProcess,
        IStoreNotification storeNotification,
        BlazorRenderer renderer,
        IDomainPostman domainPostman,
        ILoggerFactory loggerFactory)
    {
        _client = client;
        _sendGridConfiguration = sendGridConfiguration;
        _userOvasFeedForProcess = userOvasFeedForProcess;
        _storeNotification = storeNotification;
        _renderer = renderer;
        _domainPostman = domainPostman;
        _logger = loggerFactory.CreateLogger<CollectAndSendOvasNotification>();
    }

    [Function(nameof(CollectAndSendOvasNotification))]
    public async Task Run(
        [QueueTrigger(OvasCheckFeedMatchesEvent.TargetQueue, Connection = Constants.AzureConnectionName)]
        OvasCheckFeedMatchesEvent notification, CancellationToken token)
    {
        var result = await (PartitionKey.Validate(notification.PartitionKey), UserId.Validate(notification.UserId),
                EmailValidator.Validate(notification.UserEmail))
            .Apply((key, id, email) => new {PartitionKey = key, UserId = id, Email = email})
            .ValidationToEither()
            .BindAsync(safeData => _userOvasFeedForProcess
                .GetFeedForProcess(safeData.UserId, safeData.PartitionKey, token)
                .MapAsync(data => new {ProcessInfo = data, safeData.UserId, safeData.Email}))
            .MapAsync(data => SendMessage(data.ProcessInfo, data.UserId, data.Email, token));


        result.Match(
            _ => _logger.LogInformation("Notification process for Ovas feed has been completed successfully"),
            error => error.LogError(_logger)
        );
    }

    private async Task SendMessage(
        OvasUserFeed processInfo,
        UserId userId,
        Email userEmail,
        CancellationToken token)
    {
        if (processInfo.Feed.IsEmpty)
        {
            _logger.LogInformation("No ovas feed to process for {User}", userEmail);
            return;
        }

        try
        {
            var message = new SendGridMessage();
            message.SetFrom(new EmailAddress(_sendGridConfiguration.FromEmail, _sendGridConfiguration.FromName));
            message.AddTo(userEmail);
            message.SetSubject(DefaultSubject());
            message.SetSandBoxMode(_sendGridConfiguration.Sandbox);

            var html = await _renderer.RenderComponent<OvasFeed>(new Dictionary<string, object?>
            {
                {nameof(OvasFeed.Feed), processInfo.Feed}
            });

            message.AddContent(MimeType.Html, html);
            var response = await _client.SendEmailAsync(message, token);
            if (response.IsSuccessStatusCode)
            {
                var result = await _storeNotification.Add(
                    Guid.NewGuid().ToString(),
                    userId,
                    NotificationTarget.Ova,
                    NotificationArea.Feed,
                    new OvasFeedSentNotification(
                        TargetAudience.User,
                        NotificationType.Update,
                        "Ovas feed notification has been sent",
                        DateTime.Now, processInfo.Feed),
                    default).BindAsync(_ => SendCompleteMessage(processInfo.Subscriptions, token));

                result.Match(
                    _ => _logger.LogInformation("Sending ovas notification to {NotificationSubscriber}",
                        userEmail),
                    error => error.LogError(_logger)
                );
            }
            else
                _logger.LogError("Error sending email notification (Status Code {Code}) {Reason}",
                    response.StatusCode,
                    await response.Body.ReadAsStringAsync(token));
        }
        catch
            (Exception ex)
        {
            _logger.LogError(ex, "Message email sent has failed for {User}", userEmail);
        }
    }

    private async Task<Either<DomainError, Unit>> SendCompleteMessage(
        ImmutableList<OvasSubscriptionStorage> ovasSubscriptions, CancellationToken token)
    {
        var tasks = ovasSubscriptions.ConvertAll(subscription =>
            _domainPostman.SendMessage(new CompleteOvaSubscriptionEvent(subscription), token)).ToArray();

        var results = await Task.WhenAll(tasks);
        return results.FlattenResults().Map(_ => unit);
    }

    private static string DefaultSubject()
    {
        return $"Ovas subscriptions Available for Download ({DateTime.Today.ToShortDateString()})";
    }
}