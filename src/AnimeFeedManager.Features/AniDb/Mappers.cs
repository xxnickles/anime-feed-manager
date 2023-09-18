using AnimeFeedManager.Features.Common.Domain.Events;

namespace AnimeFeedManager.Features.AniDb;

internal static class AniDbMappers
{
    internal static DownloadImageEvent MapImages(SeriesContainer container, SeriesType type)
    {

        var season = container.SeasonInfo.Season;
        var seasonYear = container.SeasonInfo.Year;
        
        var partition = IdHelpers.GenerateAnimePartitionKey(season, (ushort)seasonYear);
        var directory = $"{seasonYear}/{season}";
        return new DownloadImageEvent(
            partition,
            container.Id,
            directory,
            IdHelpers.CleanAndFormatAnimeTitle(container.Title),
            container.ImageUrl ?? string.Empty,
            type);
    }
}