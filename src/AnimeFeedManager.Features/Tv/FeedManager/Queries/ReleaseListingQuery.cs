using AnimeFeedManager.Features.Tv.FeedManager.Matching;
using AnimeFeedManager.Features.Tv.FeedManager.SeriesReleases;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Queries;

/// <param name="Episodes">The library's expected count.</param>
public sealed record ReleaseListing(
    string SeriesId,
    string Title,
    string Synopsis,
    Uri? Image,
    SeriesStatus Status,
    ImmutableArray<string> OtherTitles,
    ExpectedEpisodes Episodes,
    BroadcastHint Broadcast,
    ReleaseFeed Feed);

public abstract record ReleaseFeed;

public sealed record NoReleaseFeed : ReleaseFeed;

/// <param name="Browse">nyaa's listing for the binding's search.</param>
/// <param name="Indexed">Distinct regular episodes catalogued.</param>
public sealed record NyaaReleaseFeed(
    string Group,
    string Variant,
    bool Trusted,
    Uri Browse,
    int Indexed,
    ReleaseCadence Cadence,
    ImmutableArray<EpisodeReleases> Episodes) : ReleaseFeed;

public static class ReleaseListingQuery
{
    /// <summary>A library series with its nyaa releases; an unbound series reads as <see cref="NoReleaseFeed"/>, one missing from the library fails with <see cref="NotFoundError"/>.</summary>
    public static Task<Result<ReleaseListing, DomainError>> GetReleaseListing(
        string seriesId,
        TvLibrarySeries seriesGetter,
        Uri publicBlobUri,
        NyaaBindingGetter bindingGetter,
        NyaaCatalogGetter catalogGetter,
        CancellationToken token) =>
        IdHelpers.SeriesSeasonFromId(seriesId)
            .Bind(season => seriesGetter(seriesId, season, publicBlobUri, token)
                .Map(series => (Season: season, Series: series)))
            .Bind(found => Feed(seriesId, found.Season, bindingGetter, catalogGetter, token)
                .Map(feed => ToListing(found.Series, feed)))
            .WithOperationName(nameof(GetReleaseListing))
            .WithLogProperty(Telemetry.SeriesIdTag, seriesId);

    private static Task<Result<ReleaseFeed, DomainError>> Feed(
        string seriesId,
        SeriesSeason season,
        NyaaBindingGetter bindingGetter,
        NyaaCatalogGetter catalogGetter,
        CancellationToken token) =>
        bindingGetter(seriesId, season, token)
            .Bind(binding => catalogGetter(seriesId, token)
                .Map(catalog => (ReleaseFeed) ToFeed(binding, catalog)))
            .BindOnErrorWhen(_ => new NoReleaseFeed(), error => error is NotFoundError);

    private static NyaaReleaseFeed ToFeed(NyaaBindingStorage binding, ImmutableArray<NyaaCatalogStorage> catalog) =>
        new(binding.Group,
            binding.Variant,
            binding.Trusted,
            NyaaBindingQuery.For(binding).BrowseUrl(),
            catalog.DistinctRegular(),
            NyaaReleases.Cadence(catalog),
            NyaaReleases.ByEpisode(catalog));

    private static ReleaseListing ToListing(TvSeries series, ReleaseFeed feed) =>
        new(series.Id,
            series.Title,
            series.Synopsis,
            series.Image,
            series.Status,
            OtherTitles(series),
            series.Episodes,
            series.Broadcast,
            feed);

    private static ImmutableArray<string> OtherTitles(TvSeries series) =>
    [
        ..new[] {series.AlternativeTitles.English, series.AlternativeTitles.Romaji}
            .Concat(series.AlternativeTitles.Synonyms ?? [])
            .Concat([series.AlternativeTitles.Abbreviation, series.AlternativeTitles.Native])
            .OfType<string>()
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .Where(title => !string.Equals(title, series.Title, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
    ];
}
