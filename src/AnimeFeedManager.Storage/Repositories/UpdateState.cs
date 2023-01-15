using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class UpdateState : IUpdateState
{
    private readonly TableClient _tableClient;

    public UpdateState(ITableClientFactory<UpdateStateStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public Task<Either<DomainError, string>> Create(NotificationType type, int updatesTotal)
    {
        var id = Guid.NewGuid().ToString();
        var newState = new UpdateStateStorage
        {
            RowKey = id,
            PartitionKey = type.Value,
            Completed = 0,
            Errors = 0
        };

        return TableUtils
            .TryExecute(() => _tableClient.UpsertEntityAsync(newState), nameof(UpdateStateStorage))
            .MapAsync(_ => id);
    }

    public Task<Either<DomainError, NotificationResult>> AddComplete(string id, NotificationType type)
    {
        UpdateStateStorage Add(UpdateStateStorage entity)
        {
            entity.Completed += 1;
            return entity;
        }
        
        return Get(id, type)
            .MapAsync(Add)
            .BindAsync(Merge)
            .MapAsync(Map);
        
    }

    public Task<Either<DomainError, NotificationResult>> AddError(string id, NotificationType type)
    {
        UpdateStateStorage Add(UpdateStateStorage entity)
        {
            entity.Errors += 1;
            return entity;
        }

        return Get(id, type)
            .MapAsync(Add)
            .BindAsync(Merge)
            .MapAsync(Map);
            
    }

    public Task<Either<DomainError, UpdateStateStorage>> Get(string id, NotificationType type)
    {
        return TableUtils.ExecuteQuery(() =>
                _tableClient.QueryAsync<UpdateStateStorage>(n => n.PartitionKey == type.Value && n.RowKey == id),
            nameof(UpdateStateStorage)).MapAsync(e => e.First());
    }
    
    private Task<Either<DomainError, UpdateStateStorage>> Merge(UpdateStateStorage updateStateStorage)
    {
        return TableUtils
            .TryExecute(() => _tableClient.UpsertEntityAsync(updateStateStorage), nameof(NotificationStorage))
            .MapAsync(_ => updateStateStorage);
    }

    private static NotificationResult Map(UpdateStateStorage storage)
    {
        return new NotificationResult(
            NotificationType.Parse(storage.PartitionKey),
            storage.Completed,
            storage.Errors,
            storage.Errors + storage.Completed == storage.SeriesToUpdate);
    }
}