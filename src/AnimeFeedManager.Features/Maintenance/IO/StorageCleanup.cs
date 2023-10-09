using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Notifications.Types;

namespace AnimeFeedManager.Features.Maintenance.IO
{
    public interface IStorageCleanup
    {
        public Task<Either<DomainError, Unit>> CleanOldState(NotificationTarget target, DateTimeOffset beforeOf,
            CancellationToken token);

        public Task<Either<DomainError, Unit>> CleanOldNotifications(DateTimeOffset beforeOf, CancellationToken token);
    }

    public class StorageCleanup : IStorageCleanup
    {
        private readonly ITableClientFactory<NotificationStorage> _notificationsTableClientFactory;
        private readonly ITableClientFactory<StateUpdateStorage> _stateTableClientFactory;

        public StorageCleanup(
            ITableClientFactory<NotificationStorage> notificationsTableClientFactory,
            ITableClientFactory<StateUpdateStorage> stateTableClientFactory)
        {
            _notificationsTableClientFactory = notificationsTableClientFactory;
            _stateTableClientFactory = stateTableClientFactory;
        }

        public Task<Either<DomainError, Unit>> CleanOldState(NotificationTarget target, DateTimeOffset beforeOf,
            CancellationToken token)
        {
            return _stateTableClientFactory.GetClient()
                .BindAsync(client => CleanOldState(client, target, beforeOf, token));
        }

        private static Task<Either<DomainError, Unit>> CleanOldState(
            TableClient client,
            NotificationTarget target,
            DateTimeOffset beforeOf,
            CancellationToken token)
        {
            return TableUtils.ExecuteQueryWithEmpty(
                    () => client.QueryAsync<StateUpdateStorage>(s =>
                        s.PartitionKey == target.Value && s.Timestamp <= beforeOf))
                .BindAsync(entities => TableUtils.BatchDelete(client, entities, token))
                .MapAsync(r => new Unit());
        }


        public Task<Either<DomainError, Unit>> CleanOldNotifications(DateTimeOffset beforeOf, CancellationToken token)
        {
            // Cleaning Admin Notifications Only for now
            return _notificationsTableClientFactory.GetClient()
                .BindAsync(client => CleanOldNotifications(client, beforeOf, token));
        }

        private static Task<Either<DomainError, Unit>> CleanOldNotifications(
            TableClient client,
            DateTimeOffset beforeOf,
            CancellationToken token)
        {
            return TableUtils.ExecuteQueryWithEmpty(
                    () => client.QueryAsync<NotificationStorage>(s =>
                        s.PartitionKey == UserRoles.Admin && s.Timestamp <= beforeOf))
                .BindAsync(entities =>
                    TableUtils.BatchDelete(client, entities, token))
                .MapAsync(r => new Unit());
        }
    }
}