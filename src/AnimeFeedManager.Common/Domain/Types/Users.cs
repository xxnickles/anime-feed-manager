namespace AnimeFeedManager.Common.Domain.Types;

public static class RoleNames
{
    public const string User = "User";
    public const string Admin = "Admin";
}

public enum Role
{
    User,
    Admin
}

public abstract record AppUser;
public record Anonymous() : AppUser;

public abstract record AuthenticatedUser(Email Email, UserId UserId) : AppUser;

public record User(Email Email, UserId UserId) : AuthenticatedUser(Email,UserId);

public record AdminUser(Email Email, UserId UserId) : AuthenticatedUser(Email,UserId);
