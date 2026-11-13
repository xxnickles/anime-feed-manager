using AnimeFeedManager.Features.Seasons.UpdateProcess;

namespace AnimeFeedManager.Features.Seasons.Storage;

public static class FeaturedSeason
{
    extension(SeasonStorageData data)
    {
        /// <summary>The season read paths treat as featured: the flagged one, else the newest.</summary>
        public Result<SeasonStorage, NoLatestSeason> AsFeaturedSeason() => data switch
        {
            CurrentLatestSeason latest => latest.Season,
            FallbackLatestSeason newest => newest.Season,
            _ => new NoLatestSeason()
        };
    }
}
