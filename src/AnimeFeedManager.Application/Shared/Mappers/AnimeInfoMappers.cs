using System.Collections.Immutable;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application.Shared.Mappers;

internal static class AnimeInfoMappers
{
    internal static AnimeInfoStorage ProjectToStorageModel(AnimeInfo source)
    {
        var year = OptionUtils.UnpackOption<ushort>(source.SeasonInformation.Year.Value, 0);
        return new AnimeInfoStorage
        {
            RowKey = OptionUtils.UnpackOption(source.Id.Value, string.Empty),
            PartitionKey = IdHelpers.GenerateAnimePartitionKey(source.SeasonInformation.Season, year),
            Season = source.SeasonInformation.Season.Value,
            Year = year,
            Synopsis = OptionUtils.UnpackOption(source.Synopsis.Value, string.Empty),
            FeedTitle = OptionUtils.UnpackOption(source.FeedTitle.Value, string.Empty),
            Date = MapDate(source.Date),
            Title = OptionUtils.UnpackOption(source.Title.Value, string.Empty),
            Completed = source.Completed
        };
    }
    
    internal static ImmutableList<AnimeInfoStorage> ProjectToStorageModel(ImmutableList<AnimeInfo> source) =>
        source.ConvertAll(ProjectToStorageModel);

    private static DateTime? MapDate(Option<DateTime> date)
    {
        var unpacked = date.Match(
            a => a,
            () => DateTime.MinValue
        );
        return unpacked != DateTime.MinValue ? unpacked.ToUniversalTime() : null;
    }
}