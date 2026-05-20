using System.Text.RegularExpressions;
using AnimeFeedManager.Features.Library.Import.Jikan.Types;
using AnimeFeedManager.Features.Library.Types;
using AnimeFeedManager.Shared.Results.Static;
using static AnimeFeedManager.Features.Library.Import.Jikan.Parsing.JikanParsers;

namespace AnimeFeedManager.Features.Library.Import.Jikan.Mappers;

/// <summary>
/// Maps a single <see cref="JikanAnime"/> record to its concrete <see cref="Series"/>
/// derivative. Validation errors accumulate via <see cref="Validation{T}"/>;
/// type-dispatch failure ("unknown anime type") is reported as a domain error.
/// Soft fields default when Jikan returns null/junk; hard fields (MalId, Season,
/// Year, Type) fail the mapping.
/// </summary>
public static partial class JikanSeriesMapper
{
    public static Result<Series> ToSeries(this JikanAnime jikan, DateTimeOffset lastUpdated)
    {
        var titles = BuildTitles(jikan.Titles);
        var allTitles = BuildAllTitles(titles);

        return ValidateRequired(jikan)
            .AsResult()
            .Bind(parsed => MapTypedSeries(jikan, parsed.season, parsed.year, titles, allTitles, lastUpdated));
    }

   

    // ─── type dispatch ───────────────────────────────────────────────────────

    private static Result<Series> MapTypedSeries(
        JikanAnime jikan, Season season, Year year,
        SeriesTitles titles, string[] allTitles, DateTimeOffset lastUpdated) =>
        jikan.Type?.Trim() switch
        {
            "TV" => MapTv(jikan, season, year, titles, allTitles, lastUpdated),
            "Movie" => MapMovie(jikan, season, year, titles, allTitles, lastUpdated),
            "OVA" => MapOva(jikan, season, year, titles, allTitles, lastUpdated),
            "ONA" => MapOna(jikan, season, year, titles, allTitles, lastUpdated),
            "TV Special" => MapTvSpecial(jikan, season, year, titles, allTitles, lastUpdated),
            _ => DomainValidationError.Create<Series>($"unknown anime type '{jikan.Type}'").ToErrors()
        };

    // ─── per-type mappers ────────────────────────────────────────────────────

    private static TvSeries MapTv(
        JikanAnime jikan, Season season, Year year,
        SeriesTitles titles, string[] allTitles, DateTimeOffset lastUpdated) =>
        new(jikan.MalId)
        {
            MalUrl = jikan.Url,
            Season = season,
            Year = year,
            Titles = titles,
            AllTitles = allTitles,
            Synopsis = jikan.Synopsis,
            CoverImageUrl = PickCover(jikan.Images),
            TrailerUrl = jikan.Trailer?.Url,
            Status = SeriesStatus.FromJikan(jikan.Status),
            Genres = jikan.Genres.Select(genre => genre.Name).ToArray(),
            Studios = jikan.Studios.Select(studio => studio.Name).ToArray(),
            Score = jikan.Score,
            Source = jikan.Source,
            AiredFrom = jikan.Aired?.From,
            AiredTo = jikan.Aired?.To,
            LastUpdated = lastUpdated,
            Broadcast = MapBroadcast(jikan.Broadcast),
            Episodes = jikan.Episodes,
            EpisodeDurationMinutes = ParseDurationMinutes(jikan.Duration)
        };

    private static MovieSeries MapMovie(
        JikanAnime jikan, Season season, Year year,
        SeriesTitles titles, string[] allTitles, DateTimeOffset lastUpdated) =>
        new(jikan.MalId)
        {
            MalUrl = jikan.Url,
            Season = season,
            Year = year,
            Titles = titles,
            AllTitles = allTitles,
            Synopsis = jikan.Synopsis,
            CoverImageUrl = PickCover(jikan.Images),
            TrailerUrl = jikan.Trailer?.Url,
            Status = SeriesStatus.FromJikan(jikan.Status),
            Genres = jikan.Genres.Select(genre => genre.Name).ToArray(),
            Studios = jikan.Studios.Select(studio => studio.Name).ToArray(),
            Score = jikan.Score,
            Source = jikan.Source,
            AiredFrom = jikan.Aired?.From,
            AiredTo = jikan.Aired?.To,
            LastUpdated = lastUpdated,
            RuntimeMinutes = ParseDurationMinutes(jikan.Duration)
        };

    private static OvaSeries MapOva(
        JikanAnime jikan, Season season, Year year,
        SeriesTitles titles, string[] allTitles, DateTimeOffset lastUpdated) =>
        new(jikan.MalId)
        {
            MalUrl = jikan.Url,
            Season = season,
            Year = year,
            Titles = titles,
            AllTitles = allTitles,
            Synopsis = jikan.Synopsis,
            CoverImageUrl = PickCover(jikan.Images),
            TrailerUrl = jikan.Trailer?.Url,
            Status = SeriesStatus.FromJikan(jikan.Status),
            Genres = jikan.Genres.Select(genre => genre.Name).ToArray(),
            Studios = jikan.Studios.Select(studio => studio.Name).ToArray(),
            Score = jikan.Score,
            Source = jikan.Source,
            AiredFrom = jikan.Aired?.From,
            AiredTo = jikan.Aired?.To,
            LastUpdated = lastUpdated,
            Episodes = jikan.Episodes,
            EpisodeDurationMinutes = ParseDurationMinutes(jikan.Duration)
        };

    private static OnaSeries MapOna(
        JikanAnime jikan, Season season, Year year,
        SeriesTitles titles, string[] allTitles, DateTimeOffset lastUpdated) =>
        new(jikan.MalId)
        {
            MalUrl = jikan.Url,
            Season = season,
            Year = year,
            Titles = titles,
            AllTitles = allTitles,
            Synopsis = jikan.Synopsis,
            CoverImageUrl = PickCover(jikan.Images),
            TrailerUrl = jikan.Trailer?.Url,
            Status = SeriesStatus.FromJikan(jikan.Status),
            Genres = jikan.Genres.Select(genre => genre.Name).ToArray(),
            Studios = jikan.Studios.Select(studio => studio.Name).ToArray(),
            Score = jikan.Score,
            Source = jikan.Source,
            AiredFrom = jikan.Aired?.From,
            AiredTo = jikan.Aired?.To,
            LastUpdated = lastUpdated,
            Broadcast = MapBroadcast(jikan.Broadcast),
            Episodes = jikan.Episodes,
            EpisodeDurationMinutes = ParseDurationMinutes(jikan.Duration)
        };

    private static TvSpecialSeries MapTvSpecial(
        JikanAnime jikan, Season season, Year year,
        SeriesTitles titles, string[] allTitles, DateTimeOffset lastUpdated) =>
        new(jikan.MalId)
        {
            MalUrl = jikan.Url,
            Season = season,
            Year = year,
            Titles = titles,
            AllTitles = allTitles,
            Synopsis = jikan.Synopsis,
            CoverImageUrl = PickCover(jikan.Images),
            TrailerUrl = jikan.Trailer?.Url,
            Status = SeriesStatus.FromJikan(jikan.Status),
            Genres = jikan.Genres.Select(genre => genre.Name).ToArray(),
            Studios = jikan.Studios.Select(studio => studio.Name).ToArray(),
            Score = jikan.Score,
            Source = jikan.Source,
            AiredFrom = jikan.Aired?.From,
            AiredTo = jikan.Aired?.To,
            LastUpdated = lastUpdated,
            RuntimeMinutes = ParseDurationMinutes(jikan.Duration)
        };

    // ─── helpers ─────────────────────────────────────────────────────────────

    private static SeriesTitles BuildTitles(JikanTitle[] titles)
    {
        var synonyms = titles.Where(title => title.Type == "Synonym").Select(title => title.Title).ToArray();
        return new SeriesTitles(
            Default: Pick("Default") ?? string.Empty,
            English: Pick("English"),
            Japanese: Pick("Japanese"),
            Synonyms: synonyms);
        
        string? Pick(string type) => titles.FirstOrDefault(title => title.Type == type)?.Title;
    }

    private static string[] BuildAllTitles(SeriesTitles titles)
    {
        var aggregated = new List<string>(capacity: 3 + titles.Synonyms.Length) { titles.Default };
        if (titles.English is not null) aggregated.Add(titles.English);
        if (titles.Japanese is not null) aggregated.Add(titles.Japanese);
        aggregated.AddRange(titles.Synonyms);
        return aggregated
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static Broadcast? MapBroadcast(JikanBroadcast? broadcast) =>
        broadcast is { Day: not null, Time: not null, Timezone: not null }
            ? new Broadcast(broadcast.Day, broadcast.Time, broadcast.Timezone)
            : null;

    private static string? PickCover(JikanImages? images) =>
        images?.Webp?.LargeImageUrl ?? images?.Jpg?.LargeImageUrl;

    private static int? ParseDurationMinutes(string? duration)
    {
        if (string.IsNullOrWhiteSpace(duration) || duration.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
            return null;
        var match = DurationMatcher().Match(duration);
        var hours = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
        var minutes = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        var total = hours * 60 + minutes;
        return total > 0 ? total : null;
    }

    [GeneratedRegex(@"(?:(\d+)\s*hr)?\s*(?:(\d+)\s*min)?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex DurationMatcher();
}
