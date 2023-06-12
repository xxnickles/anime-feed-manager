using AnimeFeedManager.Features.Common.Utils;

namespace AnimeFeedManager.Features.Common.Types;

public record SeasonInformation(Season Season, Year Year);

public record DefaultSeasonInformation() : SeasonInformation(Season.Winter, Year.FromNumber(2000));

public static class SeasonInformationExtensions
{
    public static ImmutableList<SeasonInformation> Order(this IEnumerable<SeasonInformation> @this)
    {
        return @this.OrderByDescending(x => x.Year.Value.UnpackOption((ushort) 0))
            .ThenByDescending(x => x.Season)
            .ToImmutableList();
    }
}

