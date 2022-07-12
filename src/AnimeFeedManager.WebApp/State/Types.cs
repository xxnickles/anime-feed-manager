namespace AnimeFeedManager.WebApp.State;

public abstract record User();

public record AnonymousUser() : User;

public record AuthenticatedUser() : User;
public record ApplicationUser(string Email, string Token) : User;