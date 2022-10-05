using System.Collections.Immutable;
using AnimeFeedManager.Application.Mappers;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Helpers;

namespace AnimeFeedManager.Application.OvasLibrary.Queries;

public sealed record OvasLibraryForStorage(
    ImmutableList<OvaStorage> Ovas,
    ImmutableList<BlobImageInfoEvent> Images,
    SeasonInfoDto Season
);

internal static class Mappers
{
    internal static OvasLibraryForStorage Map(Ovas source)
    {
        return new OvasLibraryForStorage(
            OvasMappers.ProjectToStorageModel(source.SeriesList),
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
            SeriesType.Ova
        );
    }
}