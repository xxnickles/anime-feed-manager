namespace AnimeFeedManager.Web.Features.Admin.Enpoints;

internal static class Endpoints
{
    internal static void MapAdminEndpoints(this RouteGroupBuilder group)
    {
        group.MapPut("/admin/tv/season", TvAdminHandlers.BySeason)
            .RequireAuthorization(Policies.AdminRequired);
    }
    
}