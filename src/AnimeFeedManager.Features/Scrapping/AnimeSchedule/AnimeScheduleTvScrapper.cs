using System.Net;
using System.Text.RegularExpressions;
using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Scrapping.AnimeSchedule;

internal static partial class AnimeScheduleTvScrapper
{
    private const string NoSynopsis = "No synopsis available.";
    private const string ImageBaseUrl = "https://img.animeschedule.net/production/assets/public/img/";

    internal static Task<Result<ScrapTvLibraryData>> ScrapSeries(
        this Task<Result<ImmutableArray<FeedData>>> feedTitles,
        IAnimeScheduleClient client,
        SeasonSelector selector,
        CancellationToken token) =>
        feedTitles.Bind(feedData => GetInitialProcessData(feedData, client, selector, token));

    private static Task<Result<ScrapTvLibraryData>> GetInitialProcessData(
        ImmutableArray<FeedData> feedData,
        IAnimeScheduleClient client,
        SeasonSelector selector,
        CancellationToken token) =>
        ResolveSeason(client, selector, token)
            .Bind(season => client.GetSeason(season.Year, season.Season, token)
                .Map(series => BuildResult(series, season, feedData)))
            .WithOperationName(nameof(GetInitialProcessData))
            .WithLogProperty("Season", selector)
            .AddLogOnSuccess(data => logger => logger.LogInformation(
                "{Count} TV series scraped from AnimeSchedule for {Season}",
                data.SeriesData.Count(), data.Season));

    // Scraping never elects the featured season — that is a deliberate admin action — so every
    // season produced here carries IsLatest false.
    private static Task<Result<SeriesSeason>> ResolveSeason(
        IAnimeScheduleClient client,
        SeasonSelector selector,
        CancellationToken token) =>
        selector switch
        {
            Current => client.ResolveCurrentSeason(token),
            BySeason bySeason => Task.FromResult<Result<SeriesSeason>>(
                new SeriesSeason(bySeason.Season, bySeason.Year)),
            _ => throw new UnreachableException()
        };

    private static ScrapTvLibraryData BuildResult(
        ImmutableArray<AnimeScheduleAnime> series,
        SeriesSeason season,
        ImmutableArray<FeedData> feedData) =>
        new(series
                .Where(IsTvSeries)
                .Select(anime => ToStorageData(anime, season.Season, season.Year))
                .ToArray(),
            feedData,
            season);

    // Shorts are a media type of their own here, and are part of the library.
    private static bool IsTvSeries(AnimeScheduleAnime anime) =>
        (anime.MediaTypes ?? []).Any(mediaType => mediaType.Route is "tv" or "tv-short");

    private static StorageData ToStorageData(AnimeScheduleAnime anime, Season season, Year year) =>
        new(new AnimeInfoStorage
            {
                RowKey = IdHelpers.GenerateAnimeId(season, year.ToString(), anime.Title),
                PartitionKey = IdHelpers.GenerateAnimePartitionKey(season, year),
                Title = anime.Title,
                Synopsis = Synopsis(anime.Description),
                FeedTitle = null,
                FeedLink = null,
                Date = PremierDate(anime.Premier),
                Status = SeriesStatus.NotAvailableValue
            },
            Image(anime.ImageVersionRoute),
            Status.NewSeries);

    // Descriptions carry inline markup, and sequels routinely have none at all.
    private static string Synopsis(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return NoSynopsis;

        var text = WebUtility
            .HtmlDecode(HtmlTag().Replace(LineBreakTag().Replace(description, "\n"), string.Empty))
            .Trim();

        return string.IsNullOrWhiteSpace(text) ? NoSynopsis : text;
    }

    /// <summary>Every date field uses <c>0001-01-01T00:00:00Z</c> to mean "no value".</summary>
    private static DateTime? PremierDate(DateTime? premier) =>
        premier is null || premier == DateTime.MinValue ? null : premier.Value.ToUniversalTime();

    private static ImageInformation Image(string? imageVersionRoute) =>
        !string.IsNullOrWhiteSpace(imageVersionRoute)
        && Uri.TryCreate($"{ImageBaseUrl}{imageVersionRoute}", UriKind.Absolute, out var validUri)
            ? new ScrappedImageUrl(validUri)
            : new NoImage();

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex LineBreakTag();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTag();
}
