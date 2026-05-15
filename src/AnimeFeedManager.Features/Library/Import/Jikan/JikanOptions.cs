namespace AnimeFeedManager.Features.Library.Import.Jikan;

/// <summary>
/// Configuration for the Jikan v4 client. Bind from configuration section <see cref="SectionName"/>.
/// </summary>
public sealed class JikanOptions
{
    public const string SectionName = "Jikan";

    /// <summary>
    /// Jikan v4 base URL. Defaults to the public endpoint; override for tests or self-hosted Jikan instances.
    /// Must end with a trailing slash so relative request URIs resolve correctly.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.jikan.moe/v4/";

    /// <summary>
    /// Per-attempt HTTP timeout. Applies to a single page fetch, not the full enumeration.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Max retry attempts on 408/429/5xx/HttpRequestException.
    /// Set to <c>0</c> (or any non-positive value) to skip the retry strategy entirely.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay between retries; exponential backoff and jitter are applied on top.
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);
}
