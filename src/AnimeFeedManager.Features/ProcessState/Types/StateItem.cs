namespace AnimeFeedManager.Features.ProcessState.Types;

public enum StateProcessStatus
{
    Created,
    Completed,
    Failed
}
[method: JsonConstructor]
public sealed record StateItem(string Id, StateProcessStatus Status)
{
    public static implicit operator string(StateItem stateItem) => stateItem.Id;
    public static implicit operator StateItem(string id) => new(id, StateProcessStatus.Created);
}

public static class StateItemExtensions
{
    // Extension method to convert IEnumerable<string> to IEnumerable<StateItem>
    public static IEnumerable<StateItem> ToStateItems(this IEnumerable<string> ids)
    {
        return ids.Select(id => (StateItem)id);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(StateItem))]
[JsonSerializable(typeof(StateItem[]))]
public partial class StateItemContext : JsonSerializerContext;
