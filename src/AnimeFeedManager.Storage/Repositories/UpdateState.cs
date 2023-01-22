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
            Errors = 0,
            Completed = 0,
            SeriesToUpdate = updatesTotal
        };

        return TableUtils
            .TryExecute(() => _tableClient.UpsertEntityAsync(newState), nameof(UpdateStateStorage))
            .MapAsync(_ => id);
    }

    public Task<Either<DomainError, NotificationResult>> AddComplete(string id, NotificationType type)
    {
        UpdateStateStorage Add(UpdateStateStorage original)
        {
            original.Completed += 1;
            return original;
        }

        return TryUpdate(id, type, Add);
    }

    public Task<Either<DomainError, NotificationResult>> AddError(string id, NotificationType type)
    {
        UpdateStateStorage Add(UpdateStateStorage original)
        {
            original.Errors += 1;
            return original;
        }

        return TryUpdate(id, type, Add);
    }

    private async Task<Either<DomainError, NotificationResult>> TryUpdate(string id, NotificationType type,
        Func<UpdateStateStorage, UpdateStateStorage> modifier)
    {
        var keepTrying = true;
        var finalResult =
            Left<DomainError, NotificationResult>(new BasicError($"TableOperation-{nameof(UpdateStateStorage)}",
                "Failure Updating State"));
        do
        {
            try
            {
                finalResult = await GetCurrent(id, type)
                    .MapAsync(modifier)
                    .MapAsync(Update);

                keepTrying = false;
            }
            catch (RequestFailedException e)
            {
                if (e.Status == 412) continue; // not because to pessimistic concurrency
                keepTrying = false;
                finalResult = Left<DomainError, NotificationResult>(
                    ExceptionError.FromException(e, $"TableOperation-{nameof(UpdateStateStorage)}"));
            }
            catch (Exception ex)
            {
                keepTrying = false;
                finalResult = Left<DomainError, NotificationResult>(
                    ExceptionError.FromException(ex, $"TableOperation-{nameof(UpdateStateStorage)}"));
            }
        } while (keepTrying);

        return finalResult;
    }

    private async Task<NotificationResult> Update(UpdateStateStorage updateStateStorage)
    {
        await _tableClient.UpdateEntityAsync(updateStateStorage, updateStateStorage.ETag);
        return Map(updateStateStorage);
    }


    public Task<Either<DomainError, UpdateStateStorage>> GetCurrent(string id, NotificationType type)
    {
        return TableUtils.TryExecute(() => _tableClient.GetEntityAsync<UpdateStateStorage>(type.Value, id),
                nameof(UpdateStateStorage))
            .MapAsync(r => r.Value);
    }

    private static NotificationResult Map(UpdateStateStorage storage)
    {
        return new NotificationResult(
            storage.RowKey,
            storage.Completed,
            storage.Errors,
            storage.SeriesToUpdate > 0 && storage.SeriesToUpdate == storage.Completed + storage.Errors);
    }
}