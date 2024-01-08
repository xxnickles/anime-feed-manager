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

public abstract record AuthenticatedUser(Email Email, UserId UserId, ImmutableList<NoEmptyString> TvSubscriptions) : AppUser;

public record User(Email Email, UserId UserId, ImmutableList<NoEmptyString> TvSubscriptions) : AuthenticatedUser(Email,UserId, TvSubscriptions);

public record AdminUser(Email Email, UserId UserId, ImmutableList<NoEmptyString> TvSubscriptions) : AuthenticatedUser(Email,UserId, TvSubscriptions);
