namespace AnimeFeedManager.Shared.Types;

public abstract record AppUser;
public record Anonymous : AppUser;

public sealed record TvSubscriptions(ImmutableList<string> Subscriptions, ImmutableList<string> Insterested);

public sealed record OvaSubscriptions(ImmutableList<string> Subscriptions);

public sealed record MovieSubscriptions(ImmutableList<string> Subscriptions);

public abstract record AuthenticatedUser(Email Email, NoEmptyString UserId, TvSubscriptions TvSubscriptions, OvaSubscriptions OvaSubscriptions, MovieSubscriptions MovieSubscriptions) : AppUser;

public record User(Email Email, NoEmptyString UserId, TvSubscriptions TvSubscriptions, OvaSubscriptions OvaSubscriptions, MovieSubscriptions MovieSubscriptions) : AuthenticatedUser(Email,UserId, TvSubscriptions, OvaSubscriptions, MovieSubscriptions);

public record AdminUser(Email Email, NoEmptyString UserId, TvSubscriptions TvSubscriptions, OvaSubscriptions OvaSubscriptions, MovieSubscriptions MovieSubscriptions) : AuthenticatedUser(Email,UserId, TvSubscriptions, OvaSubscriptions, MovieSubscriptions);