namespace AnimeFeedManager.Features.Domain.Types;

public record User(string Id, Email Email);

public static class UserRoles 
{
    public const string Admin = "admin";
}