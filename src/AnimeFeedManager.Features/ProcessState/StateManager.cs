using System.Text.Json;
using AnimeFeedManager.Features.ProcessState.Types;

namespace AnimeFeedManager.Features.ProcessState;

public sealed class StateManager
{
    private readonly IStateStore _stateStore;

    public StateManager(IStateStore stateStore)
    {
        _stateStore = stateStore;
    }
    
    
    public async Task<Result<Unit>> Create(StateProcessTarget target, IEnumerable<StateItem> trackingItems, string? id = null)
    {
        if (!trackingItems.Any())
            return Result<Unit>.Failure(new Error("No items to track"));

        var internalId = id ?? IdHelpers.GetUniqueId();
        var entity = new StateUpdateStorage
        {
            RowKey = internalId,
            PartitionKey = target.Value,
            Errors = 0,
            Completed = 0,
            ToUpdate = trackingItems.Count(),
            Items = JsonSerializer.Serialize(trackingItems, StateItemContext.Default.StateItemArray)
        };
        
        return await _stateStore.Upsert(entity);
    }
}