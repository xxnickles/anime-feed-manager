namespace AnimeFeedManager.Features.ProcessState.Storage;

public interface IStateStore
{
    Task<Result<Unit>> Upsert(StateUpdateStorage stateUpdateStorage,
        CancellationToken cancellationToken = default);
}

// public sealed class StateStore : IStateStore
// {
//     private readonly TableClientFactory _tableClientFactory;
//     private readonly ILogger<StateStore> _logger;
//
//     public StateStore(TableClientFactory tableClientFactory, ILogger<StateStore> logger)
//     {
//         _tableClientFactory = tableClientFactory;
//         _logger = logger;
//     }
//
//     public Task<Result<Unit>> Upsert(StateUpdateStorage stateUpdateStorage,
//         CancellationToken cancellationToken = default)
//     {
//         return _tableClientFactory.GetClient<StateUpdateStorage>(cancellationToken)
//             .TryExecute<StateUpdateStorage>(client =>
//                     client.UpsertEntityAsync(stateUpdateStorage, cancellationToken: cancellationToken),
//                 _logger)
//             .WithDefaultMap();
//     }
// }