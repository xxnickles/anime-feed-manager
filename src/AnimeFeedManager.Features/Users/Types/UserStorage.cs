namespace AnimeFeedManager.Features.Users.Types;

public enum Role
{
    User,
    Admin
}

public sealed class UserStorage : ITableEntity
{
    public string? Email { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public Role Role { get; set; } = Role.User;
}