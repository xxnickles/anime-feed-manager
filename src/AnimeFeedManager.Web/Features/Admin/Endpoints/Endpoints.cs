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

        var seasonsGroup = adminGroup.MapGroup("/seasons");
        seasonsGroup.MapPut("/featured", SeasonAdminHandlers.SetFeatured);

        var feedsGroup = adminGroup.MapGroup("/feeds");
        feedsGroup.MapPost("/bind", FeedManagerHandlers.Bind);
        feedsGroup.MapPost("/backfill", FeedManagerHandlers.Backfill);
        feedsGroup.MapPost("/clear-catalog", FeedManagerHandlers.ClearCatalog);
        feedsGroup.MapPost("/episodes/from-library", FeedManagerHandlers.UpdateEpisodesFromLibrary);
        feedsGroup.MapPost("/episodes/override", FeedManagerHandlers.OverrideEpisodes);

        var chartsGroup = adminGroup.MapGroup("/charts");
        chartsGroup.MapGet("/scrap-library", ChartHandlers.ScrapLibrarySummary);
        chartsGroup.MapGet("/notifications", ChartHandlers.NotificationSummary);
        chartsGroup.MapGet("/feed-updates", ChartHandlers.FeedUpdatesSummary);
    }
}