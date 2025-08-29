namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class Endpoints
{
    internal static void MapAdminEndpoints(this RouteGroupBuilder group)
    {
        group.MapPut("/admin/tv/season", TvAdminHandlers.BySeason)
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/tv/latest", TvAdminHandlers.Latest)
            .RequireAuthorization(Policies.AdminRequired);
    }
}