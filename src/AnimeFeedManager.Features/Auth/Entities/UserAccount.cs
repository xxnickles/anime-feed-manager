namespace AnimeFeedManager.Features.Auth.Entities;

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
