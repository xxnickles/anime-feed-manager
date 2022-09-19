using AnimeFeedManager.Core.ConstrainedTypes;

namespace AnimeFeedManager.Common.Dto;

public record SeasonInfoDto(string Season, int Year);

public record NullSeasonInfo() : SeasonInfoDto(string.Empty, 0);
public record SubscriptionDto(string UserId, string Series);

public record UserDto(string UserId, string Email);

public record ImageInformation(string Id, string Name, string? Link, SeasonInfoDto SeasonInfo);


public static class DtoFactories
{
    public static bool TryToParse(string seasonString, int year, out SeasonInfoDto season)
    {
        if (!Year.IsValid(year) || !Season.IsValid(seasonString))
        {
            season = new NullSeasonInfo();
            return false;
        }
        season = new SeasonInfoDto(seasonString, year);
        return true;

    }
}


