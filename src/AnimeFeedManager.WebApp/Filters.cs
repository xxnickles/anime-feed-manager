using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp;

public static class Filters
{
    public static bool Available(FeedAnime anime) => anime.FeedInformation.Available;
    public static bool NoAvailable(FeedAnime anime) => !anime.FeedInformation.Available;
    public static bool Completed(FeedAnime anime) => anime.FeedInformation.Completed;

    public static Func<FeedAnime, bool> Subscribed(IEnumerable<string> subscribedTitles) =>
        (FeedAnime anime) => subscribedTitles.Contains(anime.FeedInformation.Title);

    public static Func<FeedAnime, bool> Interested(IEnumerable<string> interestedTitles) =>
        (FeedAnime anime) => interestedTitles.Contains(anime.Title);

    public static IEnumerable<FeedAnime> Filter(this IEnumerable<FeedAnime> series,
        IEnumerable<Func<FeedAnime, bool>> filters)
    {
        return filters.Any()
            ? filters
                .Select(series.Where)
                .SelectMany(x => x)
            : series;
    }
}