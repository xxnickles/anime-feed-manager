using AnimeFeedManager.Features.AniDb;
using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO
{
    public sealed class LatestSeriesProvider : ILatestSeriesProvider
    {
        private readonly IDomainPostman _domainPostman;
        private readonly PuppeteerOptions _puppeteerOptions;

        public LatestSeriesProvider(
            IDomainPostman domainPostman,
            PuppeteerOptions puppeteerOptions)
        {
            _domainPostman = domainPostman;
            _puppeteerOptions = puppeteerOptions;
        }

        public async Task<Either<DomainError, TvSeries>> GetLibrary(CancellationToken token)
        {
            try
            {
                var (series, season) =
                    await AniDbScrapper.Scrap("https://anidb.net/anime/season/?type.tvseries=1", _puppeteerOptions);

                await _domainPostman.SendMessage(new SeasonProcessNotification(
                    IdHelpers.GetUniqueId(),
                    TargetAudience.Admins,
                    NotificationType.Information,
                    new SeasonInfoDto(season.Season, season.Year),
                    SeriesType.Tv,
                    $"{series.Count()} series have been scrapped for {season.Season}-{season.Year}"), token);

                return new TvSeries(series.Select(MapInfo)
                        .ToImmutableList(),
                    series.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                        .Select(seriesContainer => AniDbMappers.MapImages(seriesContainer , SeriesType.Tv))
                        .ToImmutableList());
            }
            catch (Exception ex)
            {
                await _domainPostman.SendMessage(
                    new SeasonProcessNotification(
                        IdHelpers.GetUniqueId(),
                        TargetAudience.Admins,
                        NotificationType.Error,
                        new NullSeasonInfo(),
                        SeriesType.Tv,
                        "AniDb season scrapping failed"), token);
                return ExceptionError.FromException(ex, "AniDbLibrary");
            }
        }

        private static AnimeInfoStorage MapInfo(SeriesContainer container)
        {
            var seasonInfo = MapSeasonInfo(container.SeasonInfo);
            var year = seasonInfo.Year.Value.UnpackOption((ushort)0);

            return new AnimeInfoStorage
            {
                RowKey = container.Id,
                PartitionKey = IdHelpers.GenerateAnimePartitionKey(seasonInfo.Season, year),
                Title = container.Title,
                Synopsis = container.Synopsys,
                FeedTitle = string.Empty,
                Date = MappingUtils.ParseDate(container.Date, container.SeasonInfo.Year)?.ToUniversalTime(),
                Completed = false,
                Season = seasonInfo.Season.Value,
                Year = year
            };
        }

        private static SeasonInformation MapSeasonInfo(JsonSeasonInfo jsonSeasonInfo)
        {
            return new SeasonInformation(Season.FromString(jsonSeasonInfo.Season), Year.FromNumber(jsonSeasonInfo.Year));
        }
    }
}