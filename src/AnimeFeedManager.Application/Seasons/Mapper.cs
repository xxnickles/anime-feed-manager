using System.Collections.Immutable;
using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application.Seasons;

internal static class Mapper
{
    internal static SeasonInformation Project(SeasonStorage seasonStorage)
    {
        return new SeasonInformation(Season.FromString(seasonStorage.Season), Year.FromNumber(seasonStorage.Year));
    }

    internal static ImmutableList<SeasonInformation> Project(IEnumerable<SeasonStorage> seasonStorage)
    {
        return seasonStorage.Select(Project).Order();
    }
}