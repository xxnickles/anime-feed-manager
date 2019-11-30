using System;
using System.Collections.Immutable;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Application.Shared.Mappers
{
    internal class AnimeInfoMappers
    {
        internal static AnimeInfoStorage ProjectToStorageModel(AnimeInfo source)
        {
            var year = OptionUtils.UnpackOption<ushort>(source.Year.Value, 0);
            return new AnimeInfoStorage
            {
                RowKey = OptionUtils.UnpackOption(source.Id.Value, string.Empty),
                PartitionKey = IdHelpers.GenerateAnimePartitionKey(source.Season, year),
                Season = source.Season.Value,
                Year = year,
                Synopsis = OptionUtils.UnpackOption(source.Synopsis.Value, string.Empty),
                FeedTitle = OptionUtils.UnpackOption(source.FeedTitle.Value, string.Empty),
                Date = OptionUtils.UnpackOption<DateTime?>((DateTime?)source.Date, null),
                Title = OptionUtils.UnpackOption(source.Title.Value, string.Empty),
            };
        }

        internal static ImmutableList<AnimeInfoStorage> ProjectToStorageModel(ImmutableList<AnimeInfo> source) =>
            source.ConvertAll(ProjectToStorageModel);

        internal static AnimeInfoStorage ProjectToStorageModelWithEtag(AnimeInfo source) => ProjectToStorageModel(source).AddEtag();

        internal static ImmutableList<AnimeInfoStorage> ProjectToStorageModelWithEtag(ImmutableList<AnimeInfo> source) =>
            source.ConvertAll(ProjectToStorageModelWithEtag);
    }
}