namespace AnimeFeedManager.WebApp.State
{
    public abstract record User();

    public record AnonymousUser() : User;

    public record AuthenticatedUser(string Id) : User;

    public record ApplicationUser(string Id) : User;

    public record AdminUser(string Id) : ApplicationUser(Id);
}