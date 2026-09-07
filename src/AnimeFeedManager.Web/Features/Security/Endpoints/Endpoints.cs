namespace AnimeFeedManager.Web.Features.Security.Endpoints;

internal static class Endpoints
{
    internal static void MapSecurityEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/create-token", SecurityHandlers.CreateToken);

        group.MapPost("/add-credential", SecurityHandlers.AddCredential);
        group.MapPost("/login", SecurityHandlers.LoginUser);
        group.MapGet("/logout", SecurityHandlers.Logout);
    }
}