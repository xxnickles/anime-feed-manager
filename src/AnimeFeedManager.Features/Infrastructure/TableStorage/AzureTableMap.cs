namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

internal static class AzureTableMap
{
    internal readonly struct StoreTo
    {
        internal const string AnimeLibrary = "AnimeLibrary";
        internal const string Subscriptions = "Subscriptions";
        internal const string AvailableSeasons = "AvailableSeasons";
        internal const string ProcessedSubscriptions = "ProcessedSubscriptions";
        internal const string Users = "Users";
        internal const string Events = "Events";
        internal const string JsonStorage = "JsonTable";
        internal const string NyaaBindings = "NyaaBindings";
        internal const string NyaaCatalog = "NyaaCatalog";
    }
}