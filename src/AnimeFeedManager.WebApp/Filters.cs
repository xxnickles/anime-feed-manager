using AnimeFeedManager.Features.Common.Domain.Types;
using AnimeFeedManager.Features.Common.Dto;

namespace AnimeFeedManager.WebApp;

public static class Filters
{
    public static bool Available(FeedAnime anime) => anime.FeedInformation.Available;
    public static bool NoAvailable(FeedAnime anime) => !anime.FeedInformation.Available;
    public static bool Completed(FeedAnime anime) => anime.FeedInformation.Status == SeriesStatus.Completed;

    public static Func<FeedAnime, bool> Subscribed(IEnumerable<string> subscribedTitles) =>
        anime => subscribedTitles.Contains(anime.FeedInformation.Title);

    public static Func<FeedAnime, bool> Interested(IEnumerable<string> interestedTitles) =>
        anime => interestedTitles.Contains(anime.Title);

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