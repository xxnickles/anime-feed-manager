using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Types;
using AnimeFeedManager.Features.Domain;

namespace AnimeFeedManager.Features.AniDb;

internal static class AniDbMappers
{
    internal static ImageInformation MapImages(SeriesContainer container)
    {
        return new ImageInformation(
            container.Id,
            IdHelpers.CleanAndFormatAnimeTitle(container.Title),
            container.ImageUrl,
            new SeasonInformation(Season.FromString(container.SeasonInfo.Season),
                Year.FromNumber(container.SeasonInfo.Year)));
    }
}