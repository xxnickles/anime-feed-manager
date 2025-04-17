using System.Collections.Frozen;
using System.Text.Json.Serialization;
using AnimeFeedManager.Old.Common.Dto;

namespace AnimeFeedManager.Old.Features.Seasons.Types;

public abstract record SeasonInfo(Season Season);

public record NotAvailableSeason(Season Season) : SeasonInfo(Season);

public record LatestSeason(Season Season) : SeasonInfo(Season);

public record RegularSeason(Season Season) : SeasonInfo(Season);

public record SeasonGroup(Year Year, ImmutableList<SeasonInfo> Seasons, bool HasLatest);

public record SeasonWrapper(Season Season, Year Year, bool IsLatest);

public record SimpleSeasonWrapper(string Season, int Year, bool IsLatest);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SimpleSeasonWrapper[]))]
public partial class SimpleSeasonWrapperContext : JsonSerializerContext;

public static class Extensions
{
    private static readonly FrozenSet<Season> YearSeasons =
        new[] { Season.Winter, Season.Spring, Season.Summer, Season.Fall }.ToFrozenSet();

    public static SeasonGroup ToGroup(
        this IEnumerable<SeasonWrapper> seasonWrappers, Year year)
    {
        var values = YearSeasons.Aggregate((Seasons: new List<SeasonInfo>(), HasLatest: false), (acc, next) =>
        {
            SeasonInfo seasonInfo = seasonWrappers.FirstOrDefault(s => s.Season == next) switch
            {
                { IsLatest: true } => new LatestSeason(next),
                not null => new RegularSeason(next),
                _ => new NotAvailableSeason(next)
            };

            acc.Seasons.Add(seasonInfo);
            if (seasonInfo is LatestSeason)
                acc.HasLatest = true;

            return acc;
        });

        return new SeasonGroup(year, values.Seasons.ToImmutableList(), values.HasLatest);
    }

    public static SimpleSeasonWrapper ToSimple(this SeasonWrapper season) =>
        new(season.Season, season.Year, season.IsLatest);

    public static SeasonWrapper ToWrapper(this SimpleSeasonWrapper season) =>
        new(Season.FromString(season.Season), Year.FromNumber(season.Year), season.IsLatest);


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