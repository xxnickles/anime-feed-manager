namespace AnimeFeedManager.Shared.Types;

public abstract record AppUser;
public record Anonymous : AppUser;

public abstract record AuthenticatedUser(Email Email, NoEmptyString UserId) : AppUser;

public record RegularUser(Email Email, NoEmptyString UserId) : AuthenticatedUser(Email,UserId);

public record AdminUser(Email Email, NoEmptyString UserId) : AuthenticatedUser(Email,UserId);