namespace AnimeFeedManager.Features.Domain.Types;

public abstract record SeasonSelector;

public record Latest : SeasonSelector;

public record BySeason(SimpleSeasonInfo SeasonInfo) : SeasonSelector;

public static class Extensions
{
    public static bool IsLatest(this SeasonSelector seasonSelector) => seasonSelector switch
    {
        Latest => true,
        _ => false
    };

}