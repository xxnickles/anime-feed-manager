namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class Endpoints
{
    internal static void MapAdminEndpoints(this RouteGroupBuilder group)
    {
        var adminGroup = group.MapGroup("/admin/tv")
            .RequireAuthorization(Policies.AdminRequired);

        adminGroup.MapPut("/season", TvAdminHandlers.BySeason);

        adminGroup.MapPut("/latest", TvAdminHandlers.Latest);

        adminGroup.MapPut("/titles", TvAdminHandlers.Titles);
        
        adminGroup.MapPost("/run-notifications", TvAdminHandlers.TriggerNotificationProcess);
    }
}