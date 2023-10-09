using System.Text.Json.Serialization;

namespace AnimeFeedManager.Common.RealTimeNotifications
{
    public record ConnectionInfo(string Url, string AccessToken);

    [JsonSerializable(typeof(ConnectionInfo))]
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    public partial class ConnectionInfoContext : JsonSerializerContext
    {
    }
}