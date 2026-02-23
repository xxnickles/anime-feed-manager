using AnimeFeedManager.Features.Infrastructure.Email;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;
using AnimeFeedManager.Web.BlazorComponents.Email.Templates;

namespace AnimeFeedManager.Functions.Tv.Notifications;

public class OnTvFeedNotification
{
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly IEmailNotificationSender _emailSender;
    private readonly ILogger<OnTvFeedNotification> _logger;

    public OnTvFeedNotification(
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        IEmailNotificationSender emailSender,
        ILogger<OnTvFeedNotification> logger)
    {
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _emailSender = emailSender;
        _logger = logger;
    }

    [Function(nameof(OnTvFeedNotification))]
    public async Task Run(
        [QueueTrigger(FeedNotification.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        FeedNotification notification,
        CancellationToken cancellationToken)
    {
        await _emailSender.Send(
                notification.Subscriptions.UserEmail,
                $"Subscriptions Available for Download ({DateTime.Today.ToShortDateString()})",
                NotificationEmail.AsRenderFragment,
                notification.ToEmailModel(),
                cancellationToken)
            .Bind(_ => NotificationProcess.UpdateUserSubscriptions(notification,
                _tableClientFactory.TableStorageTvSubscriptionsUpdater,
                _domainPostman.SendMessages,
                cancellationToken))
            .AddLogOnSuccess(summary => logger => logger.LogInformation("{NotificationCount} have been sent to {User}", summary.NotificationsSent, summary.UserId))
            .Complete(_logger);
    }
}