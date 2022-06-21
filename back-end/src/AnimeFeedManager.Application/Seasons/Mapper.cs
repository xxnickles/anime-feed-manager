using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AnimeFeedManager.Application.Seasons;

internal static class Mapper
{
    internal static SeasonInformation Project(SeasonStorage seasonStorage)
    {
        return new SeasonInformation(Season.FromString(seasonStorage.Season), new Year(seasonStorage.Year));
    }

    internal static ImmutableList<SeasonInformation> Project(IEnumerable<SeasonStorage> seasonStorage)
    {
        return seasonStorage.Select(Project).Order();
    }
}