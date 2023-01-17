using System.Collections.Immutable;
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
            RowKey = Guid.NewGuid().ToString(),
            PartitionKey = type.Value,
            Type = UpdateType.Created,
            SeriesToUpdate = updatesTotal,
            StateGroup =  id
        };

        return TableUtils
            .TryExecute(() => _tableClient.UpsertEntityAsync(newState), nameof(UpdateStateStorage))
            .MapAsync(_ => id);
    }

    public Task<Either<DomainError, Unit>> AddComplete(string id, NotificationType type)
    {
        return Merge(new UpdateStateStorage
        {
            RowKey = Guid.NewGuid().ToString(),
            StateGroup = id,
            PartitionKey = type.Value,
            Type = UpdateType.Complete,
        });
    }

    public Task<Either<DomainError, Unit>> AddError(string id, NotificationType type)
    {
        return Merge(new UpdateStateStorage
        {
            RowKey = Guid.NewGuid().ToString(),
            StateGroup = id,
            PartitionKey = type.Value,
            Type = UpdateType.Error,
        });
    }

    public Task<Either<DomainError, NotificationResult>> GetCurrent(string id, NotificationType type)
    {
        return TableUtils.ExecuteQuery(() =>
                _tableClient.QueryAsync<UpdateStateStorage>(n => n.PartitionKey == type.Value && n.StateGroup == id),
            nameof(UpdateStateStorage)).MapAsync(Map);
    }

    private Task<Either<DomainError, Unit>> Merge(UpdateStateStorage updateStateStorage)
    {
        return TableUtils
            .TryExecute(() => _tableClient.UpsertEntityAsync(updateStateStorage), nameof(NotificationStorage))
            .MapAsync(_ => new Unit());
      
    }

    private static NotificationResult Map(ImmutableList<UpdateStateStorage> storage)
    {
        var total = storage.FirstOrDefault(x => x.Type == UpdateType.Created)?.SeriesToUpdate ?? 0;
        var completed = storage.Count(x => x.Type == UpdateType.Complete);
        var errors = storage.Count(x => x.Type == UpdateType.Error);
        
        return new NotificationResult(
            completed,
            errors,
            total > 0 && completed + errors == total);
    }
}