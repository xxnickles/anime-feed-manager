using AnimeFeedManager.Features.Tv.FeedManager.Staleness;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Queries;

public sealed record FeedManagerSeries(
    string SeriesId,
    SeriesSeason Season,
    string Title,
    SeriesFeed Feed,
    ExpectedEpisodes LibraryEpisodes,
    ImmutableArray<string> TitlePresets,
    ImmutableArray<StreamLink> Streams,
    CurrentBinding Binding);

public abstract record CurrentBinding;

public sealed record NoBinding : CurrentBinding;

/// <summary><paramref name="Indexed"/> counts distinct regular episodes in the catalog.</summary>
public sealed record ActiveBinding(
    string Group,
    string ReleaseTitle,
    string Variant,
    bool Trusted,
    bool Tracking,
    ExpectedEpisodes Episodes,
    DateTimeOffset? CompletedAt,
    int Indexed,
    LatestRelease? Latest,
    bool Stale) : CurrentBinding;

public sealed record LatestRelease(string Episode, DateTimeOffset Published);

public static class FeedManagerSeriesQuery
{
    /// <summary>A library series with its binding and catalog summary; a series never bound reads as <see cref="NoBinding"/>.</summary>
    public static Task<Result<FeedManagerSeries>> GetFeedManagerSeries(
        string seriesId,
        TvSeriesGetter seriesGetter,
        NyaaBindingGetter bindingGetter,
        NyaaCatalogGetter catalogGetter,
        DateTimeOffset now,
        CancellationToken token) =>
        IdHelpers.SeriesSeasonFromId(seriesId)
            .Bind(season => seriesGetter(seriesId, season, token).Map(series => (Season: season, Series: series)))
            .Bind(found => Binding(seriesId, found.Season, bindingGetter, catalogGetter, now, token)
                .Map(binding => ToSeries(seriesId, found.Season, found.Series, binding)))
            .WithOperationName(nameof(GetFeedManagerSeries))
            .WithLogProperty(Telemetry.SeriesIdTag, seriesId);

    private static Task<Result<CurrentBinding>> Binding(
        string seriesId,
        SeriesSeason season,
        NyaaBindingGetter bindingGetter,
        NyaaCatalogGetter catalogGetter,
        DateTimeOffset now,
        CancellationToken token) =>
        bindingGetter(seriesId, season, token)
            .Bind(binding => catalogGetter(seriesId, token)
                .Map(catalog => (CurrentBinding) ToActive(binding, catalog, now)))
            .BindOnErrorWhen(_ => new NoBinding(), error => error is NotFoundError);

    private static ActiveBinding ToActive(
        NyaaBindingStorage binding,
        ImmutableArray<NyaaCatalogStorage> catalog,
        DateTimeOffset now) =>
        new(binding.Group,
            binding.ReleaseTitle,
            binding.Variant,
            binding.Trusted,
            binding.Tracking,
            SeriesBroadcast.ToExpectedEpisodes(binding.Episodes),
            binding.CompletedAt,
            catalog.Where(row => !row.Special).Select(row => (int) row.EpisodeNumber).Distinct().Count(),
            catalog.MaxBy(row => row.Published) is { } newest
                ? new LatestRelease(newest.EpisodeLabel, newest.Published)
                : null,
            NyaaStaleness.IsStale(binding, catalog, now));

    private static FeedManagerSeries ToSeries(
        string seriesId,
        SeriesSeason season,
        AnimeInfoStorage series,
        CurrentBinding binding) =>
        new(seriesId,
            season,
            series.Title ?? string.Empty,
            series.Feed,
            SeriesBroadcast.ToExpectedEpisodes(series.Episodes),
            TitlePresets(series),
            SeriesBroadcast.ToBroadcastHint(series.SubTime, series.Date, series.Streams).Where,
            binding);

    private static ImmutableArray<string> TitlePresets(AnimeInfoStorage series)
    {
        var titles = StoredAlternativeTitles.Parse(series.AlternativeTitles);
        return
        [
            ..new[] {series.Title, titles.English, titles.Romaji}
                .Concat(titles.Synonyms ?? [])
                .Concat(titles.User ?? [])
                .Concat([titles.Abbreviation, titles.Native])
                .OfType<string>()
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Distinct(StringComparer.OrdinalIgnoreCase)
        ];
    }
}
