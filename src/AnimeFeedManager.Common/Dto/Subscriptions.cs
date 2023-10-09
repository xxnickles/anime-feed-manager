using System.Text.Json.Serialization;

namespace AnimeFeedManager.Common.Dto;

public record SimpleTvSubscription(string UserId, string Series);

[JsonSerializable(typeof(SimpleTvSubscription))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SimpleTvSubscriptionContext : JsonSerializerContext
{
}


public record ShortSeriesSubscription(string UserId, string Series, DateTime NotificationDate);

[JsonSerializable(typeof(ShortSeriesSubscription))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ShortSeriesSubscriptionContext : JsonSerializerContext
{
}

public record ShortSeriesUnsubscribe(string UserId, string Series);

[JsonSerializable(typeof(ShortSeriesUnsubscribe))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ShortSeriesUnsubscribeContext : JsonSerializerContext
{
}