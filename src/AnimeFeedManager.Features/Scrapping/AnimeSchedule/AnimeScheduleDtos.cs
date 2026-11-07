namespace AnimeFeedManager.Features.Scrapping.AnimeSchedule;

internal sealed record AnimeScheduleResponse(
    int Page,
    int TotalAmount,
    AnimeScheduleAnime[] Anime);

/// <summary>
/// <paramref name="Status"/> is the provider's airing state: <c>Upcoming</c>, <c>Ongoing</c>,
/// <c>Finished</c> or <c>Delayed</c>. The set is not closed — treat anything else as unknown.
/// <para>Every date field uses <c>0001-01-01T00:00:00Z</c> as "no value".</para>
/// </summary>
public sealed record AnimeScheduleAnime(
    string Id,
    string Title,
    string? Description,
    string? ImageVersionRoute,
    DateTime? Premier,
    AnimeScheduleSeason? Season,
    AnimeScheduleNames? Names,
    AnimeScheduleMediaType[]? MediaTypes,
    string Status,
    int? Episodes,
    DateTime? JpnTime,
    DateTime? SubTime,
    AnimeScheduleWebsites? Websites);

/// <summary>
/// <paramref name="Year"/> is a string here, unlike the int <c>year</c> on the anime itself.
/// </summary>
public sealed record AnimeScheduleSeason(
    string? Title,
    string? Year,
    string? Season,
    string? Route);

/// <summary>
/// Every slot is optional, and the object itself is absent on roughly 1% of records. A slot is
/// either a real value or missing — the API never sends an empty string or an empty list — so
/// <c>null</c> is the only "no value" shape to defend against.
/// <para><paramref name="Romaji"/> repeats the canonical <c>title</c> verbatim wherever it is present.</para>
/// </summary>
public sealed record AnimeScheduleNames(
    string? Romaji,
    string? English,
    string? Native,
    string[]? Synonyms,
    string? Abbreviation);

public sealed record AnimeScheduleWebsites(AnimeScheduleStream[]? Streams);

/// <summary>
/// Match on <paramref name="Platform"/> (lowercase key, e.g. <c>crunchyroll</c>) — <paramref name="Name"/> casing is inconsistent.
/// <paramref name="Url"/> is series-specific and has no scheme.
/// </summary>
public sealed record AnimeScheduleStream(
    string? Platform,
    string? Url,
    string? Name);

/// <summary>
/// Match on <paramref name="Route"/> (<c>tv</c>, <c>tv-short</c>) — display names carry inconsistent casing.
/// </summary>
public sealed record AnimeScheduleMediaType(
    string? Name,
    string? Route);
