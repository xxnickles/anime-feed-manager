namespace AnimeFeedManager.Features.Auth.Entities;

/// <summary>
/// System-wide registry of users — one denormalized <c>{email, userId, role}</c> entry per
/// account. Lives in the <c>system</c> container alongside other system singletons (mirrors
/// <c>LibrarySeasonsIndex</c>), read as one record. Backs the registration email-dedup check
/// and a future admin user list. Read via read-all-in-partition so a later spill to multiple
/// index documents stays transparent to callers (spill logic itself is not built yet).
/// </summary>
[CosmosEntity(CosmosContainers.System, "/partitionKey")]
public sealed record UsersIndex : SystemDocument
{
    public const string DocumentId = "users-index";

    public ImmutableArray<UserIndexEntry> Users { get; init; } = ImmutableArray<UserIndexEntry>.Empty;
}

/// <summary>Denormalized registry entry: enough to dedup by email and resolve a user id + role.</summary>
public sealed record UserIndexEntry(string Email, string UserId, string Role);
