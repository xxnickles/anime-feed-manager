namespace AnimeFeedManager.Features.Common.Domain.Types;

public abstract record SeasonSelector;

public record Latest : SeasonSelector;

public record BySeason(string Season, int Year) : SeasonSelector;

public static class Extensions
{
    public static bool IsLatest(this SeasonSelector seasonSelector) => seasonSelector switch
    {
        Latest => true,
        _ => false
    };

}