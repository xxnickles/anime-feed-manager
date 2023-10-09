namespace AnimeFeedManager.Common.Types;

public record SeasonInformation(Season Season, Year Year);

public record DefaultSeasonInformation() : SeasonInformation(Season.Winter, default);

// public static class SeasonInformationExtensions
// {
//     public static ImmutableList<SeasonInformation> Order(this IEnumerable<SeasonInformation> @this)
//     {
//         return @this.OrderByDescending(x => x.Year.Value)
//             .ThenByDescending(x => x.Season)
//             .ToImmutableList();
//     }
// }