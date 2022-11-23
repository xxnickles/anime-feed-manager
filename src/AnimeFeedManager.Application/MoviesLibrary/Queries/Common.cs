using System.Collections.Immutable;
using AnimeFeedManager.Application.Mappers;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;

namespace AnimeFeedManager.Application.MoviesLibrary.Queries;

public sealed record MoviesLibraryForStorage(
    ImmutableList<MovieStorage> Movies,
    ImmutableList<BlobImageInfoEvent> Images,
    SeasonInfoDto Season
);

internal static class Mappers
{
    internal static MoviesLibraryForStorage Map(Movies source)
    {
        return new MoviesLibraryForStorage(
            MoviesMappers.ProjectToStorageModel(source.SeriesList),
            Map(source.Images),
            source.Images.First().SeasonInfo.Map()
        );
    }

    private static ImmutableList<BlobImageInfoEvent> Map(ImmutableList<ImageInformation> source)
    {
        return source.ConvertAll(Map);
    }

    private static BlobImageInfoEvent Map(ImageInformation source)
    {
        var season = source.SeasonInfo.Map();
        var partition = IdHelpers.GenerateAnimePartitionKey(season.Season, (ushort)season.Year);
        var directory = $"{season.Year}/{season.Season}";
        return new BlobImageInfoEvent(
            partition,
            source.Id,
            directory,
            source.Name,
            source.Link ?? string.Empty,
            SeriesType.Movie
        );
    }
}