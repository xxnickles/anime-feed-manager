namespace AnimeFeedManager.Features.Scrapping.AnimeSchedule;

internal sealed record AnimeScheduleResponse(
    int Page,
    int TotalAmount,
    AnimeScheduleAnime[] Anime);

public sealed record AnimeScheduleAnime(
    string Id,
    string Title,
    string? Description,
    string? ImageVersionRoute,
    DateTime? Premier,
    AnimeScheduleSeason? Season,
    AnimeScheduleNames? Names,
    AnimeScheduleMediaType[]? MediaTypes);

/// <summary>
/// <paramref name="Year"/> is a string here, unlike the int <c>year</c> on the anime itself.
/// </summary>
public sealed record AnimeScheduleSeason(
    string? Title,
    string? Year,
    string? Season,
    string? Route);

/// <summary>
/// Every slot is optional, and the object itself is absent on roughly 5% of records. A slot is
/// either a real value or missing — the API never sends an empty string or an empty list — so
/// <c>null</c> is the only "no value" shape to defend against.
/// <para><paramref name="Romaji"/> repeats the canonical <c>title</c> verbatim wherever it is present.</para>
/// </summary>
public sealed record AnimeScheduleNames(
    string? Romaji,
    string? English,
    string? Native,
    string[]? Synonyms);

/// <summary>
/// Match on <paramref name="Route"/> (<c>tv</c>, <c>tv-short</c>) — display names carry inconsistent casing.
/// </summary>
public sealed record AnimeScheduleMediaType(
    string? Name,
    string? Route);
