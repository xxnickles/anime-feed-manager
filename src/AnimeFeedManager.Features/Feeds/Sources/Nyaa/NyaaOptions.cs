namespace AnimeFeedManager.Features.Feeds.Sources.Nyaa;

/// <summary>
/// Configuration for the Nyaa RSS client. Bind from configuration section <see cref="SectionName"/>.
/// </summary>
public sealed class NyaaOptions
{
    public const string SectionName = "Nyaa";

    /// <summary>Nyaa base URL. Must end with a trailing slash so relative request URIs resolve correctly.</summary>
    public string BaseUrl { get; set; } = "https://nyaa.si/";

    /// <summary>Category filter (<c>c=</c>) — defaults to Anime - English-translated.</summary>
    public string Category { get; set; } = "1_2";

    /// <summary>Trust filter (<c>f=</c>) — defaults to trusted-only.</summary>
    public string Filter { get; set; } = "2";

    /// <summary>
    /// Per-attempt HTTP timeout for the single feed fetch. Retry/circuit-breaker behavior comes
    /// from the app-wide standard resilience handler (<c>AddWebAppDefaults</c>) — Nyaa publishes
    /// no rate limit and this client makes one flat request per run, so no custom pipeline is needed.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
