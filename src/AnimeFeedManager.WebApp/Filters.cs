using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp;

public static class Filters
{
    public static bool Available(SimpleAnime anime) => anime.FeedInformation.Available;
    public static bool NoAvailable(SimpleAnime anime) => !anime.FeedInformation.Available;
    public static bool Completed(SimpleAnime anime) => anime.FeedInformation.Completed;

    public static Func<SimpleAnime, bool> Subscribed(IEnumerable<string> subscribedTitles) =>
        (SimpleAnime anime) => subscribedTitles.Contains(anime.FeedInformation.Title);

    public static Func<SimpleAnime, bool> Interested(IEnumerable<string> interestedTitles) =>
        (SimpleAnime anime) => interestedTitles.Contains(anime.Title);

    public static IEnumerable<SimpleAnime> Filter(this IEnumerable<SimpleAnime> series,
        IEnumerable<Func<SimpleAnime, bool>> filters)
    {
        return filters.Any()
            ? filters
                .Select(series.Where)
                .SelectMany(x => x)
            : series;
    }
}