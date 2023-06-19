using AnimeFeedManager.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Features.State.Types;

namespace AnimeFeedManager.Features.State.IO;

public sealed class CreateState : ICreateState
{
    private readonly ITableClientFactory<StateUpdateStorage> _tableClientFactory;

    public CreateState(ITableClientFactory<StateUpdateStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<StateWrap<T>>>> Create<T>(StateUpdateTarget target,
        ImmutableList<T> entities)
    {
        var id = Guid.NewGuid().ToString();
        var newState = new StateUpdateStorage
        {
            RowKey = id,
            PartitionKey = target.Value,
            Errors = 0,
            Completed = 0,
            SeriesToUpdate = entities.Count
        };

        return _tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.TryExecute(() => client.UpsertEntityAsync(newState), nameof(StateUpdateStorage)))
            .MapAsync(_ => entities.ConvertAll(e => new StateWrap<T>(id, e)));
    }
}