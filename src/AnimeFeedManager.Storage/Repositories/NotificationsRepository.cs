using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class NotificationsRepository : INotificationsRepository
{
    private readonly TableClient _tableClient;
    private readonly JsonSerializerOptions _serializerOptions;

    public NotificationsRepository(ITableClientFactory<NotificationStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        _serializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }

    public Task<Either<DomainError, ImmutableList<NotificationStorage>>> Get(string userId)
    {
        return TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<NotificationStorage>(n => n.PartitionKey == userId),
            nameof(NotificationStorage));
    }

    public Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForAdmin(string userId)
    {
        return TableUtils.ExecuteQuery(() =>
                _tableClient.QueryAsync<NotificationStorage>(n =>
                    n.PartitionKey == userId && n.PartitionKey == UserRoles.Admin),
            nameof(NotificationStorage));
    }

    public Task<Either<DomainError, Unit>> Merge<T>(string userId, NotificationType type, T payload)
    {
        var notificationStorage = new NotificationStorage
        {
            PartitionKey = type != NotificationType.Admin ? userId : UserRoles.Admin,
            RowKey = Guid.NewGuid().ToString(),
            Payload = JsonSerializer.Serialize(payload, _serializerOptions),
            Type = type.Value
        };
        return TableUtils
            .TryExecute(() => _tableClient.UpsertEntityAsync(notificationStorage), nameof(NotificationStorage))
            .MapAsync(_ => unit);
    }
}