using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.Notifications.Types;

namespace AnimeFeedManager.Features.Notifications.IO;

public class StoreNotification : IStoreNotification
{
    private readonly ITableClientFactory<NotificationStorage> _tableClientFactory;
    private readonly JsonSerializerOptions _serializerOptions;
    public StoreNotification(ITableClientFactory<NotificationStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
        _serializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }
    
    public Task<Either<DomainError, Unit>> Add<T>(string id, string userId, NotificationTarget target, NotificationArea area, T payload,
        CancellationToken token)
    {

       return _tableClientFactory.GetClient()
            .BindAsync(client =>
            {
                var notificationStorage = new NotificationStorage
                {
                    PartitionKey = target != NotificationTarget.Admin ? userId : UserRoles.Admin,
                    RowKey = id,
                    Payload = JsonSerializer.Serialize(payload, _serializerOptions),
                    Type = area.Value,
                    For = @target.Value
                };

                return TableUtils.TryExecute(
                        () => client.UpsertEntityAsync(notificationStorage, cancellationToken: token),
                        nameof(NotificationStorage))
                    .MapAsync(_ => unit);
            });
    }

  
}