namespace AnimeFeedManager.Features.Auth.Events;

/// <summary>
/// Raised once a user is fully registered (account + registry written). A fire-and-forget domain
/// event; the admin notification channel that consumes it is deferred to the SSE channels breakdown.
/// </summary>
public sealed record UserRegistered(string UserId, Email Email);
