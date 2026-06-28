namespace AnimeFeedManager.Features.Auth.Storage;

/// <summary>
/// Storage-layer view of a user lookup: a record either exists and parses cleanly
/// (<see cref="ValidStoredUser"/>) or it doesn't (<see cref="NotAStoredUser"/>).
/// Absence is a successful outcome, not an error — callers pattern-match on it.
/// </summary>
public abstract record StoredUser;

public sealed record ValidStoredUser(Email Email, NoEmptyString UserId, UserRole Role) : StoredUser;

public sealed record NotAStoredUser : StoredUser;
