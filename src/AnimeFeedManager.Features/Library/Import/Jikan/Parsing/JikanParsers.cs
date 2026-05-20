using AnimeFeedManager.Features.Library.Import.Jikan.Types;
using AnimeFeedManager.Shared.Results.Static;

namespace AnimeFeedManager.Features.Library.Import.Jikan.Parsing;

internal static class JikanParsers
{
    internal static Validation<(Season season, Year year)> ValidateRequired(JikanAnime jikan) =>
        ValidateSeason(jikan.Season)
            .And(ValidateYear(jikan.Year))
            .And(ValidateMalId(jikan.MalId))
            .Map(t => (season: t.Item1, year: t.Item2));

    private static Validation<Season> ValidateSeason(string? season) =>
        season is null
            ? Validation<Season>.Invalid("season is missing")
            : season.ParseAsSeason();

    private static Validation<Year> ValidateYear(int? year) =>
        year is null
            ? Validation<Year>.Invalid("year is missing")
            : year.Value.ParseAsYear();

    private static Validation<Unit> ValidateMalId(int malId) =>
        malId > 0
            ? Validation<Unit>.Valid(new Unit())
            : Validation<Unit>.Invalid($"mal_id must be positive (got {malId})");
}