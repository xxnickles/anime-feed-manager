using System.Collections.Immutable;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application.Mappers;

internal static class OvasMappers
{
    internal static OvaStorage ProjectToStorageModel(ShortAnimeInfo source)
    {
        var year = source.SeasonInformation.Year.Value.UnpackOption<ushort>(0);
        return new OvaStorage
        {
            RowKey = source.Id.Value.UnpackOption(string.Empty),
            PartitionKey = IdHelpers.GenerateAnimePartitionKey(source.SeasonInformation.Season, year),
            Season = source.SeasonInformation.Season.Value,
            Year = year,
            Synopsis = source.Synopsis.Value.UnpackOption(string.Empty),
            Date = CommonMappers.MapDate(source.Date),
            Title = source.Title.Value.UnpackOption(string.Empty),
        };
    }
    
    internal static ImmutableList<OvaStorage> ProjectToStorageModel(ImmutableList<ShortAnimeInfo> source) =>
        source.ConvertAll(ProjectToStorageModel);
}