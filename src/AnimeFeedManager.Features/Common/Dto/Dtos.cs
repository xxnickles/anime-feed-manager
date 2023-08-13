namespace AnimeFeedManager.Features.Common.Dto;

public record SimpleSeasonInfo(string Season, int Year, bool IsLatest);

public record NullSimpleSeasonInfo() : SimpleSeasonInfo(string.Empty, 0,false);
public record TvSubscriptionDto(string UserId, string Series);

public record ShortSeriesSubscriptionDto(string UserId, string Series, DateTime NotificationDate);
public record ShortSeriesUnsubscribeDto(string UserId, string Series);

public record UserDto(string UserId, string Email);

public static class DtoFactories
{
    public static bool TryToParse(string seasonString, int year, bool isLatest, out SimpleSeasonInfo simpleSeason)
    {
        if (!Year.IsValid(year) || !Season.IsValid(seasonString))
        {
            simpleSeason = new NullSimpleSeasonInfo();
            return false;
        }
        simpleSeason = new SimpleSeasonInfo(seasonString, year, isLatest);
        return true;

    }
}