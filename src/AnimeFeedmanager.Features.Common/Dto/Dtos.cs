using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Common.Dto;

public record SimpleSeasonInfo(string Season, int Year, bool IsLatest);

[JsonSerializable(typeof(SimpleSeasonInfo))]
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