using AnimeFeedManager.Features.Domain.Events;

namespace AnimeFeedManager.Features.AniDb;

internal static class AniDbMappers
{
    internal static DownloadImageEvent MapImages(SeriesContainer container, SeriesType type)
    {
        var season = DtoMappers.Map(
            new SeasonInformation(Season.FromString(container.SeasonInfo.Season),
                Year.FromNumber(container.SeasonInfo.Year)));
            
        var partition = IdHelpers.GenerateAnimePartitionKey(season.Season, (ushort)season.Year);
        var directory = $"{season.Year}/{season.Season}";
        return new DownloadImageEvent(
            partition,
            container.Id,
            directory,
            IdHelpers.CleanAndFormatAnimeTitle(container.Title),
            container.ImageUrl ?? string.Empty,
            type);
    }
}