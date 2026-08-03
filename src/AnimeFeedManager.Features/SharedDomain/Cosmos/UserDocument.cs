using AnimeFeedManager.Features.Auth.Entities;
using AnimeFeedManager.Features.Subscriptions.Entities;

namespace AnimeFeedManager.Features.SharedDomain.Cosmos;

/// <summary>
/// Polymorphic base for every document in the <c>users</c> container. Partitioned by
/// <see cref="UserId"/> (the Passwordless handle), so a user's account and all their
/// per-user data (subscriptions, and future per-user concerns) share one logical partition.
/// New per-user document kinds slot in as <see cref="JsonDerivedTypeAttribute"/> entries and
/// are read back as the right concrete type via the <c>docType</c> discriminator.
/// </summary>
[CosmosEntity(CosmosContainers.Users, "/userId")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "docType")]
[JsonDerivedType(typeof(UserAccount), "account")]
[JsonDerivedType(typeof(UserSubscription), "subscription")]
public abstract record UserDocument : CosmosDocument
{
    /// <summary>Partition key value — the Passwordless user id. Serializes to <c>userId</c>.</summary>
    public string UserId { get; init; } = string.Empty;
}
