using System.Text.Json.Serialization;

namespace AnimeFeedManager.Common.Dto;

public record BasicSeason(string Season, ushort Year);
public record ShorSeriesLatestSeason(bool KeeepFeed);

public record ShorSeriesSeason(string Season, ushort Year, bool KeeepFeed);

public record SimpleSeasonInfo(string Season, int Year, bool IsLatest);

[JsonSerializable(typeof(SimpleSeasonInfo[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SimpleSeasonInfoContext : JsonSerializerContext
{
}

public record NullSimpleSeasonInfo() : SimpleSeasonInfo(string.Empty, 0,false);

public static class DtoFactories
{
    public static bool TryToParse(string seasonString, int year, bool isLatest, out SimpleSeasonInfo simpleSeason)
    {
        if (!Year.NumberIsValid(year) || !Season.IsValid(seasonString))
        {
            simpleSeason = new NullSimpleSeasonInfo();
            return false;
        }
        simpleSeason = new SimpleSeasonInfo(seasonString, year, isLatest);
        return true;

    }
}