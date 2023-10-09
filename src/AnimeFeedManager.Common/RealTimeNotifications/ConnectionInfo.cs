using System.Text.Json.Serialization;

namespace AnimeFeedManager.Common.RealTimeNotifications;

public record ConnectionInfo(string AccessToken,string Url);

[JsonSerializable(typeof(ConnectionInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ConnectionInfoContext : JsonSerializerContext
{
}