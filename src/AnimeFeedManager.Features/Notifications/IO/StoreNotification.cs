using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Notifications.Types;
using Notification = AnimeFeedManager.Features.Common.Domain.Notifications.Base.Notification;

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