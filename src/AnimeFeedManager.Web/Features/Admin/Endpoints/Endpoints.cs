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
        tvGroup.MapPost("/run-notifications", TvAdminHandlers.TriggerNotificationProcess);
        tvGroup.MapPost("/run-automated-binding", TvAdminHandlers.TriggerAutomatedBinding);

        var seasonsGroup = adminGroup.MapGroup("/seasons");
        seasonsGroup.MapPut("/featured", SeasonAdminHandlers.SetFeatured);

        var feedsGroup = adminGroup.MapGroup("/feeds");
        feedsGroup.MapPost("/bind", FeedManagerHandlers.Bind);
        feedsGroup.MapPost("/backfill", FeedManagerHandlers.Backfill);
        feedsGroup.MapPost("/clear-catalog", FeedManagerHandlers.ClearCatalog);
        feedsGroup.MapPost("/episodes/from-library", FeedManagerHandlers.UpdateEpisodesFromLibrary);
        feedsGroup.MapPost("/episodes/override", FeedManagerHandlers.OverrideEpisodes);
        feedsGroup.MapGet("/series/{seriesId}/cards", FeedManagerHandlers.Cards);
        feedsGroup.MapGet("/series/{seriesId}/candidates", FeedManagerHandlers.Candidates);

        var chartsGroup = adminGroup.MapGroup("/charts");
        chartsGroup.MapGet("/scrap-library", ChartHandlers.ScrapLibrarySummary);
        chartsGroup.MapGet("/notifications", ChartHandlers.NotificationSummary);
        chartsGroup.MapGet("/feed-updates", ChartHandlers.FeedUpdatesSummary);

        var statsGroup = adminGroup.MapGroup("/stats");
        statsGroup.MapGet("/last-scrape", StatHandlers.LastScrape);
        statsGroup.MapGet("/errors", StatHandlers.Errors);
        statsGroup.MapGet("/nyaa", StatHandlers.Nyaa);
        statsGroup.MapGet("/automated-binding", StatHandlers.AutomatedBinding);
    }
}