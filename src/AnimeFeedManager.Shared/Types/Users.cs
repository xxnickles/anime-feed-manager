namespace AnimeFeedManager.Shared.Types;

public closed record AppUser;
public record Anonymous : AppUser;

public closed record AuthenticatedUser(Email Email, NoEmptyString UserId) : AppUser;

public record RegularUser(Email Email, NoEmptyString UserId) : AuthenticatedUser(Email,UserId);

public record AdminUser(Email Email, NoEmptyString UserId) : AuthenticatedUser(Email,UserId);