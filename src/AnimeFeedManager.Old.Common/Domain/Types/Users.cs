using Email = AnimeFeedManager.Old.Common.Types.Email;

namespace AnimeFeedManager.Old.Common.Domain.Types;

public static class RoleNames
{
    public const string User = "User";
    public const string Admin = "Admin";
}


public abstract record AppUser;
public record Anonymous : AppUser;

public sealed record TvSubscriptions(ImmutableList<string> Subscriptions, ImmutableList<string> Insterested);

public sealed record OvaSubscriptions(ImmutableList<string> Subscriptions);

public sealed record MovieSubscriptions(ImmutableList<string> Subscriptions);

public abstract record AuthenticatedUser(Email Email, UserId UserId, TvSubscriptions TvSubscriptions, OvaSubscriptions OvaSubscriptions, MovieSubscriptions MovieSubscriptions) : AppUser;

public record User(Email Email, UserId UserId, TvSubscriptions TvSubscriptions, OvaSubscriptions OvaSubscriptions, MovieSubscriptions MovieSubscriptions) : AuthenticatedUser(Email,UserId, TvSubscriptions, OvaSubscriptions, MovieSubscriptions);

public record AdminUser(Email Email, UserId UserId, TvSubscriptions TvSubscriptions, OvaSubscriptions OvaSubscriptions, MovieSubscriptions MovieSubscriptions) : AuthenticatedUser(Email,UserId, TvSubscriptions, OvaSubscriptions, MovieSubscriptions);
