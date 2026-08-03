using AnimeFeedManager.Features.Subscriptions.Entities;

namespace AnimeFeedManager.Features.Subscriptions;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> for the Subscriptions feature's Cosmos
/// documents. Options mirror <c>AddCosmosInfrastructure</c> so stream-based reads/writes that
/// bypass the Cosmos serializer produce identical JSON.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(UserDocument))]
[JsonSerializable(typeof(UserSubscription))]
public partial class SubscriptionsJsonContext : JsonSerializerContext;
