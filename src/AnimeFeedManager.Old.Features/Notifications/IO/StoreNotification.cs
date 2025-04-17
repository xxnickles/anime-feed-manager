using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Notifications.Types;
using Notification = AnimeFeedManager.Old.Common.Domain.Notifications.Base.Notification;

namespace AnimeFeedManager.Old.Features.Notifications.IO;

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