using System.Collections.Immutable;
using AnimeFeedManager.Core.Domain;

namespace AnimeFeedManager.Core.Utils;

public static class SeasonInformationExtensions
{
    public static ImmutableList<SeasonInformation> Order(this IEnumerable<SeasonInformation> @this)
    {
        return @this.OrderByDescending(x => OptionUtils.UnpackOption(x.Year.Value, (ushort) 0))
            .ThenByDescending(x => x.Season)
            .ToImmutableList();
    }
}