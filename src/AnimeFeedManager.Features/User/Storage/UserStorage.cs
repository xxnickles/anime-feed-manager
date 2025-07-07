namespace AnimeFeedManager.Features.User.Storage;

[WithTableName(AzureTableMap.StoreTo.Users)]
public sealed class UserStorage : ITableEntity
{
    public string Email { get; set; } = string.Empty;
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Role { get; set; } = UserRole.None();
}