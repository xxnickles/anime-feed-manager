using System.Text.Json;
using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.Notifications.Types;
using Notification = AnimeFeedManager.Features.Domain.Notifications.Notification;

namespace AnimeFeedManager.Features.Notifications.IO;

public interface IStoreNotification
{
    public Task<Either<DomainError, Unit>> Add<T>(string id, string userId, NotificationTarget target,
        NotificationArea area, T payload, CancellationToken token) where T : Notification;
}

public class StoreNotification : IStoreNotification
{
    private readonly ITableClientFactory<NotificationStorage> _tableClientFactory;

    public StoreNotification(ITableClientFactory<NotificationStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, Unit>> Add<T>(string id, string userId, NotificationTarget target,
        NotificationArea area,
        T payload,
        CancellationToken token) where T : Notification
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client =>
            {
                var notificationStorage = new NotificationStorage
                {
                    PartitionKey = userId,
                    RowKey = id,
                    Payload = GetSerializedPayload(payload),
                    Type = area.Value,
                    For = target.Value
                };

                return TableUtils.TryExecute(
                        () => client.UpsertEntityAsync(notificationStorage, cancellationToken: token))
                    .MapAsync(_ => unit);
            });
    }

    private static string GetSerializedPayload(Notification payload) => payload switch
    {
        ImageUpdateNotification imageUpdateNotification => JsonSerializer.Serialize(imageUpdateNotification,
            ImageUpdateNotificationContext.Default.ImageUpdateNotification),
        SeasonProcessNotification seasonProcessNotification => JsonSerializer.Serialize(seasonProcessNotification,
            SeasonProcessNotificationContext.Default.SeasonProcessNotification),
        AutomatedSubscriptionProcessNotification automatedSubscriptionProcessNotification => JsonSerializer.Serialize(
            automatedSubscriptionProcessNotification,
            AutomatedSubscriptionProcessNotificationContext.Default.AutomatedSubscriptionProcessNotification),
        _ => JsonSerializer.Serialize(payload, NotificationContext.Default.Notification)
    };
}