namespace AnimeFeedManager.Features.Users.Types;

public abstract record AppUser;

public record Anonymous() : AppUser;

public record User(Email Email, UserId UserId) : AppUser;

public record AdminUser(Email Email, UserId UserId) : AppUser;
