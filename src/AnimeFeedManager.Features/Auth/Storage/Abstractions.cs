using AnimeFeedManager.Features.Auth.Entities;

namespace AnimeFeedManager.Features.Auth.Storage;

/// <summary>
/// Point-reads a user's account by id (the Passwordless handle): <c>Read(id: "account", pk: userId)</c>.
/// A missing account is a successful <see cref="NotAStoredUser"/>, not an error. Drives login.
/// </summary>
public delegate Task<Result<StoredUser>> UserAccountGetter(
    NoEmptyString userId, CancellationToken cancellationToken = default);

/// <summary>
/// Resolves a user from the registry by email — the dedup gate at registration. A missing entry
/// is a successful <see cref="NotAStoredUser"/>.
/// </summary>
public delegate Task<Result<StoredUser>> UserByEmailGetter(
    Email email, CancellationToken cancellationToken = default);

/// <summary>Upserts a user's account document into its <c>userId</c> partition.</summary>
public delegate Task<Result<Unit>> UserAccountUpserter(
    Email email, string userId, UserRole role, string displayName, CancellationToken cancellationToken = default);

/// <summary>
/// Adds (or replaces by user id) a registry entry under ETag optimistic concurrency:
/// load-merge-upsert, reloading and retrying on a concurrent-write conflict.
/// </summary>
public delegate Task<Result<Unit>> UsersIndexRegistrar(
    UserIndexEntry entry, CancellationToken cancellationToken = default);
