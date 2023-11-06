using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Notifications.Types;
using Notification = AnimeFeedManager.Common.Domain.Notifications.Base.Notification;

namespace AnimeFeedManager.Features.Notifications.IO;

public interface IStoreNotification
{
    public Task<Either<DomainError, Unit>> Add<T>(string id, string userId, NotificationTarget target,
        NotificationArea area, T payload, CancellationToken token) where T : Notification;
}

public class StoreNotification(ITableClientFactory<NotificationStorage> tableClientFactory)
    : IStoreNotification
{
    public Task<Either<DomainError, Unit>> Add<T>(string id, string userId, NotificationTarget target,
        NotificationArea area,
        T payload,
        CancellationToken token) where T : Notification
    {
        return tableClientFactory.GetClient()
            .BindAsync(client =>
            {
                var notificationStorage = new NotificationStorage
                {
                    PartitionKey = userId,
                    RowKey = id,
                    Payload = payload.GetSerializedPayload(),
                    Type = area.Value,
                    For = target.Value
                };

                return TableUtils.TryExecute(
                        () => client.UpsertEntityAsync(notificationStorage, cancellationToken: token))
                    .MapAsync(_ => unit);
            });
    }
}