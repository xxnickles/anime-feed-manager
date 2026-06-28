namespace AnimeFeedManager.Features.Auth.Entities;

internal static class Constants
{
    /// <summary>
    /// Partition key for the <c>users</c> container: the Passwordless user id.
    /// Co-locates a user's account with their future per-user data (subscriptions).
    /// </summary>
    internal const string UsersPartitionKey = "/userId";
}
