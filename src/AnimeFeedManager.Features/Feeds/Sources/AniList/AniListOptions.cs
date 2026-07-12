namespace AnimeFeedManager.Features.Feeds.Sources.AniList;

/// <summary>
/// Configuration for the AniList GraphQL client. Bind from configuration section <see cref="SectionName"/>.
/// </summary>
public sealed class AniListOptions
{
    public const string SectionName = "AniList";

    /// <summary>AniList GraphQL endpoint. Must end with a trailing slash.</summary>
    public string BaseUrl { get; set; } = "https://graphql.anilist.co/";

    /// <summary>
    /// Per-attempt HTTP timeout. Retry/circuit-breaker behavior comes from the app-wide standard
    /// resilience handler (<c>AddWebAppDefaults</c>) — this runs a handful of paginated requests
    /// once daily, well under AniList's 90/min limit even without a custom pipeline.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Results per page for the batched <c>idMal_in</c> query.</summary>
    public int PageSize { get; set; } = 50;
}
