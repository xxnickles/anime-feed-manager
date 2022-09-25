namespace AnimeFeedManager.DI;

internal static class AzureTable
{
    internal struct TableMap
    {
        internal const string AnimeLibrary = "AnimeLibrary";
        internal const string Subscriptions = "Subscriptions";
        internal const string AvailableSeasons = "AvailableSeasons";
        internal const string InterestedSeries = "InterestedSeries";
        internal const string FeedTitles = "FeedTitles";
        internal const string ProcessedTitles = "ProcessedTitles";
        internal const string Users = "Users";
        internal const string OvaLibrary = "OvaLibrary";
        internal const string MovieLibrary = "MovieLibrary";
    }
}