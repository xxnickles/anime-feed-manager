using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Seasons.Types;

public record SeasonWrapper(Season Season, Year Year, bool IsLatest);

public record SimpleSeasonWrapper(string Season, int Year, bool IsLatest);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SimpleSeasonWrapper[]))]
public partial class SimpleSeasonWrapperContext : JsonSerializerContext
{
}

public static class Extensions
{
    public static SimpleSeasonWrapper ToSimple(this SeasonWrapper season) =>
        new(season.Season, season.Year, season.IsLatest);

    public static SeasonWrapper ToWrapper(this SimpleSeasonWrapper season) =>
        new(Season.FromString(season.Season), Year.FromNumber(season.Year), season.IsLatest);


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