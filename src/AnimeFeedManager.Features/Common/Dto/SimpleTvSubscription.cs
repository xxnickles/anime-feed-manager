using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Common.Dto;

public record SimpleTvSubscription(string UserId, string Series);

[JsonSerializable(typeof(SimpleTvSubscription))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SimpleTvSubscriptionContext : JsonSerializerContext
{
}