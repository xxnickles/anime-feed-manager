using AnimeFeedManager.Common;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class StorageCleanup : IStorageCleanup
{
    private readonly TableClient _stateTableClient;
    private readonly TableClient _notificationsTableClient;

    public StorageCleanup(
        ITableClientFactory<NotificationStorage> notificationsTableClientFactory,
        ITableClientFactory<UpdateStateStorage> stateTableClientFactory)
    {
        _stateTableClient = stateTableClientFactory.GetClient();
        _notificationsTableClient = notificationsTableClientFactory.GetClient();

        _stateTableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        _notificationsTableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public Task<Either<DomainError, Unit>> CleanOldState()
    {
        return TableUtils.ExecuteQueryWithEmpty(
                () => _stateTableClient.QueryAsync<UpdateStateStorage>(s => s.Timestamp <= DateTime.Now.AddDays(-7)),
                nameof(UpdateStateStorage))
            .BindAsync(entities => TableUtils.BatchDelete(_stateTableClient, entities, nameof(UpdateStateStorage)))
            .MapAsync(r => new Unit());
    }

    public Task<Either<DomainError, Unit>> CleanOldNotifications()
    {
        // Cleaning Admin Notifications Only for now
        return TableUtils.ExecuteQueryWithEmpty(
                () => _notificationsTableClient.QueryAsync<NotificationStorage>(s =>
                    s.PartitionKey == UserRoles.Admin && s.Timestamp <= DateTime.Now.AddDays(-30)),
                nameof(UpdateStateStorage))
            .BindAsync(entities => TableUtils.BatchDelete(_stateTableClient, entities, nameof(UpdateStateStorage)))
            .MapAsync(r => new Unit());
    }
}