using AnimeFeedManager.Features.Infrastructure.TableStorage;

namespace AnimeFeedManager.Features.ProcessState.Storage;

public interface IStateStore
{
    ValueTask<Result<Unit>> Upsert(StateUpdateStorage stateUpdateStorage,
        CancellationToken cancellationToken = default);
}

public sealed class StateStore : IStateStore
{
    private readonly TableClientFactory<StateUpdateStorage> _tableClientFactory;
    private readonly ILogger<StateStore> _logger;

    public StateStore(TableClientFactory<StateUpdateStorage> tableClientFactory, ILogger<StateStore> logger)
    {
        _tableClientFactory = tableClientFactory;
        _logger = logger;
    }

    public ValueTask<Result<Unit>> Upsert(StateUpdateStorage stateUpdateStorage,
        CancellationToken cancellationToken = default)
    {
        return _tableClientFactory.GetClient(cancellationToken)
            .TryExecute(client =>
                    client.UpsertEntityAsync(stateUpdateStorage, cancellationToken: cancellationToken),
                _logger)
            .WithDefaultMap();
    }
}