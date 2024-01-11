namespace AnimeFeedManager.Common.Domain.Types;

public static class RoleNames
{
    public const string User = "User";
    public const string Admin = "Admin";
}


public abstract record AppUser;
public record Anonymous() : AppUser;

public record TvSubscriptions(ImmutableList<string> Subscriptions, ImmutableList<string> Insterested);

public abstract record AuthenticatedUser(Email Email, UserId UserId, TvSubscriptions TvSubscriptions) : AppUser;

public record User(Email Email, UserId UserId, TvSubscriptions TvSubscriptions) : AuthenticatedUser(Email,UserId, TvSubscriptions);

public record AdminUser(Email Email, UserId UserId, TvSubscriptions TvSubscriptions) : AuthenticatedUser(Email,UserId, TvSubscriptions);
