namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class Endpoints
{
    internal static void MapAdminEndpoints(this RouteGroupBuilder group)
    {
        var adminGroup = group.MapGroup("/admin")
            .RequireAuthorization(Policies.AdminRequired);

        var tvGroup = adminGroup.MapGroup("/tv");
        tvGroup.MapPut("/season", TvAdminHandlers.BySeason);
        tvGroup.MapPut("/latest", TvAdminHandlers.Latest);
        tvGroup.MapPut("/titles", TvAdminHandlers.Titles);
        tvGroup.MapPost("/run-notifications", TvAdminHandlers.TriggerNotificationProcess);

        var chartsGroup = adminGroup.MapGroup("/charts");
        chartsGroup.MapGet("/scrap-library", ChartHandlers.ScrapLibrarySummary);
        chartsGroup.MapGet("/notifications", ChartHandlers.NotificationSummary);
        chartsGroup.MapGet("/feed-updates", ChartHandlers.FeedUpdatesSummary);
    }
}