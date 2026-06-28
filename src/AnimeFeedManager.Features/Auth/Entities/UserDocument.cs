using static AnimeFeedManager.Features.Auth.Entities.Constants;

namespace AnimeFeedManager.Features.Auth.Entities;

/// <summary>
/// Polymorphic base for every document in the <c>users</c> container. Partitioned by
/// <see cref="UserId"/> (the Passwordless handle), so a user's account and all their
/// per-user data share one logical partition. New per-user document kinds (e.g.
/// subscriptions) slot in as <see cref="JsonDerivedTypeAttribute"/> entries and are
/// read back as the right concrete type via the <c>docType</c> discriminator.
/// </summary>
[CosmosEntity(CosmosContainers.Users, UsersPartitionKey)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "docType")]
[JsonDerivedType(typeof(UserAccount), "account")]
public abstract record UserDocument : CosmosDocument
{
    /// <summary>Partition key value — the Passwordless user id. Serializes to <c>userId</c>.</summary>
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// The canonical account record for a user. One per partition, addressed by the fixed
/// <see cref="DocumentId"/> id so login is a point-read <c>(id: "account", pk: userId)</c>.
/// Stores primitives (typed at the boundary on read) per the no-required/sentinel-default
/// rule for Cosmos entities.
/// </summary>
public sealed record UserAccount : UserDocument
{
    public const string DocumentId = "account";

    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Role { get; init; } = UserRole.None().ToString();

    public UserAccount()
    {
        Id = DocumentId;
    }
}
