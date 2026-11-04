namespace AnimeFeedManager.Web.Features.Admin;

internal static class AdminTelemetry
{
    public const string UpdateTvSeasonSpan = "Admin.UpdateTvSeason";
    public const string UpdateLatestTvSeasonSpan = "Admin.UpdateLatestTvSeason";
    public const string TriggerFeedNotificationsSpan = "Admin.TriggerFeedNotifications";
    public const string TriggerAutomatedBindingSpan = "Admin.TriggerAutomatedBinding";
    public const string SetFeaturedSeasonSpan = "Admin.SetFeaturedSeason";
    public const string LibraryScrapSummarySpan = "Admin.LibraryScrapSummary";
    public const string NotificationSummarySpan = "Admin.NotificationSummary";
    public const string FeedUpdatesSummarySpan = "Admin.FeedUpdatesSummary";
    public const string LastScrapeStatSpan = "Admin.LastScrapeStat";
    public const string ErrorStatSpan = "Admin.ErrorStat";
    public const string NyaaStatSpan = "Admin.NyaaStat";
    public const string AutomatedBindingStatSpan = "Admin.AutomatedBindingStat";
    public const string BindNyaaSeriesSpan = "Admin.BindNyaaSeries";
    public const string BackfillNyaaSeriesSpan = "Admin.BackfillNyaaSeries";
    public const string ClearNyaaCatalogSpan = "Admin.ClearNyaaCatalog";
    public const string UpdateFeedEpisodesSpan = "Admin.UpdateFeedEpisodes";
    public const string OverrideEpisodesSpan = "Admin.OverrideEpisodes";
    public const string RefreshBindingCardsSpan = "Admin.RefreshBindingCards";
    public const string RefreshReleaseCandidatesSpan = "Admin.RefreshReleaseCandidates";
}
