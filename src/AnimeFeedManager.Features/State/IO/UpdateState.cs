using AnimeFeedManager.Features.State.Types;

namespace AnimeFeedManager.Features.State.IO;

public sealed class UpdateState : IUpdateState
{
    private readonly ITableClientFactory<StateUpdateStorage> _tableClientFactory;

    public UpdateState(ITableClientFactory<StateUpdateStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, CurrentState>> UpdateCompleted(string id, StateUpdateTarget target,
        CancellationToken token = default)
    {
        StateUpdateStorage Add(StateUpdateStorage original)
        {
            original.Completed += 1;
            return original;
        }

        return _tableClientFactory.GetClient()
            .BindAsync(client => TryUpdate(client, id, target, Add, token));
    }

    public Task<Either<DomainError, CurrentState>> UpdateError(string id, StateUpdateTarget target,
        CancellationToken token = default)
    {
        StateUpdateStorage Add(StateUpdateStorage original)
        {
            original.Errors += 1;
            return original;
        }

        return _tableClientFactory.GetClient()
            .BindAsync(client => TryUpdate(client, id, target, Add, token));
    }


    private static async Task<Either<DomainError, CurrentState>> TryUpdate(
        TableClient client,
        string id,
        StateUpdateTarget target,
        Func<StateUpdateStorage, StateUpdateStorage> modifier,
        CancellationToken token)
    {
        var keepTrying = true;
        var finalResult =
            Left<DomainError, CurrentState>(
                new BasicError($"TableOperation-{nameof(StateUpdateStorage)}",
                    "Failure Updating State"));
        do
        {
            try
            {
                finalResult = await  GetCurrent(client, id, target)
                    .MapAsync(modifier)
                    .MapAsync(s => Update(client, s, token));

                keepTrying = false;
            }
            catch (RequestFailedException e)
            {
                if (e.Status == 412) continue; // not because to pessimistic concurrency
                keepTrying = false;
                finalResult = Left<DomainError, CurrentState>(
                    ExceptionError.FromException(e, $"TableOperation-{nameof(StateUpdateStorage)}"));
            }
            catch (Exception ex)
            {
                keepTrying = false;
                finalResult = Left<DomainError, CurrentState>(
                    ExceptionError.FromException(ex, $"TableOperation-{nameof(StateUpdateStorage)}"));
            }
        } while (keepTrying);

        return finalResult;
    }

    private static Task<Either<DomainError, StateUpdateStorage>> GetCurrent(
        TableClient client,
        string id,
        StateUpdateTarget target)
    {
        return TableUtils.TryExecute(() => client.GetEntityAsync<StateUpdateStorage>(target.Value, id),
                nameof(StateUpdateStorage))
            .MapAsync(r => r.Value);
    }

    private static async Task<CurrentState> Update(
        TableClient client,
        StateUpdateStorage updateStateStorage,
        CancellationToken token)
    {
        await client.UpdateEntityAsync(updateStateStorage, updateStateStorage.ETag, cancellationToken: token);
        return new CurrentState(
            updateStateStorage.RowKey!,
            updateStateStorage.Completed,
            updateStateStorage.Errors,
            updateStateStorage.SeriesToUpdate > 0 && updateStateStorage.SeriesToUpdate ==
            updateStateStorage.Completed + updateStateStorage.Errors);
    }
}