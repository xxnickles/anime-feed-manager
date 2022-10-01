using System.Collections.Immutable;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application.Mappers;

internal static class AnimeInfoMappers
{
    internal static AnimeInfoStorage ProjectToStorageModel(AnimeInfo source)
    {
        var year = source.SeasonInformation.Year.Value.UnpackOption<ushort>(0);
        return new AnimeInfoStorage
        {
            RowKey = source.Id.Value.UnpackOption(string.Empty),
            PartitionKey = IdHelpers.GenerateAnimePartitionKey(source.SeasonInformation.Season, year),
            Season = source.SeasonInformation.Season.Value,
            Year = year,
            Synopsis = source.Synopsis.Value.UnpackOption(string.Empty),
            FeedTitle = source.FeedTitle.Value.UnpackOption(string.Empty),
            Date = CommonMappers.MapDate(source.Date),
            Title = source.Title.Value.UnpackOption(string.Empty),
            Completed = source.Completed
        };
    }

    internal static ImmutableList<AnimeInfoStorage> ProjectToStorageModel(ImmutableList<AnimeInfo> source) =>
        source.ConvertAll(ProjectToStorageModel);
}