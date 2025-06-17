namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

internal static class AzureTableMap
{
    internal readonly struct StoreTo
    {
        internal const string AnimeLibrary = "AnimeLibrary";
        internal const string AlternativeTitles = "AlternativeTitles";
        internal const string Subscriptions = "Subscriptions";
        internal const string AvailableSeasons = "AvailableSeasons";
        internal const string InterestedSeries = "InterestedSeries";
        internal const string FeedTitles = "FeedTitles";
        internal const string ProcessedTitles = "ProcessedTitles";
        internal const string Users = "Users";
        internal const string OvaLibrary = "OvaLibrary";
        internal const string MovieLibrary = "MovieLibrary";
        internal const string MovieSubscriptions = "MovieSubscriptions";
        internal const string OvaSubscriptions = "OvaSubscriptions";
        internal const string Events = "Events";
        internal const string StateUpdates = "StateUpdates";
        internal const string JsonStorage = "JsonTable";
    }
}