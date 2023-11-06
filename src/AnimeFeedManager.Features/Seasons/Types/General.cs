namespace AnimeFeedManager.Features.Seasons.Types;

public record struct SeasonWrapper(Season Season, Year Year, bool IsLatest);

public static class Extensions
{
    public static SeasonInformation ToSeasonInformation(this SeasonWrapper season)
    {
        return new SeasonInformation(season.Season, season.Year);
    }
    
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