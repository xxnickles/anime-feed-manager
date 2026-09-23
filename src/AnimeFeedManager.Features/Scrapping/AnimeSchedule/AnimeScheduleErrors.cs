namespace AnimeFeedManager.Features.Scrapping.AnimeSchedule;

/// <summary>
/// A 429 from AnimeSchedule. The public tier sends no rate-limit headers and bans the caller's
/// whole IP range for hours, so this is terminal: never retried, and never folded into a partial
/// result, since every page left unfetched would be silently missing from it.
/// </summary>
internal sealed record RateLimitedError : DomainError
{
    private RateLimitedError(string message) : base(message)
    {
    }

    public static RateLimitedError Create(string query, int page) =>
        new($"AnimeSchedule rate-limited {query} at page {page}");

    public override Action<ILogger> LogAction() =>
        logger => logger.LogError("{Message}. Further requests will be refused for hours", Message);
}
