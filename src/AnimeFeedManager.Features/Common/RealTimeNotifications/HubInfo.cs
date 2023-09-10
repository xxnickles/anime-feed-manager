using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Common.RealTimeNotifications;

public record HubInfo(string ConnectionId);

[JsonSerializable(typeof(HubInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class HubInfoContext : JsonSerializerContext
{
}