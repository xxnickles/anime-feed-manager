namespace AnimeFeedManager.Features.Seasons.Types;

internal record struct SeasonWrapper(Season Season, Year Year, bool IsLatest);

internal static class Extensions
{
    internal static SeasonWrapper ToWrapper(this SeasonStorage storage)
    {
        return new SeasonWrapper(
            Season.FromString(storage.Season),
            Year.FromNumber(storage.Year),
            storage.Latest
        );
    }

    internal static SimpleSeasonInfo ToSimpleSeason(this SeasonWrapper season)
    {
        return new SimpleSeasonInfo(
            season.Season,
            season.Year,
            season.IsLatest
        );
    }
}