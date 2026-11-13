using System.Net;

namespace AnimeFeedManager.Features.Scrapping;

public closed record ScrapeError(string Message) : DomainError(Message)
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);

    /// <summary>
    /// A 429 from AnimeSchedule. The public tier sends no rate-limit headers and bans the caller's whole IP range
    /// for hours, so it is terminal: never retried, never folded into a partial result.
    /// </summary>
    public sealed record RateLimited(string Query, int Page)
        : ScrapeError($"AnimeSchedule rate-limited {Query} at page {Page}")
    {
        public override Action<ILogger> LogAction() =>
            logger => logger.LogError("{domain_error.message}. Further requests will be refused for hours", Message);
    }

    /// <summary>An HTML page in place of the Nyaa feed: the DDoS-Guard challenge, which the client cannot pass.</summary>
    public sealed record Challenged(string Request, HttpStatusCode Status)
        : ScrapeError($"Nyaa answered {Request} with an HTML page (status {(int) Status}), likely a DDoS-Guard challenge");

    /// <summary>A non-JSON body in place of the SubsPlease schedule: a challenge page or a site change.</summary>
    public sealed record SourceUnavailable(string Request, HttpStatusCode Status)
        : ScrapeError($"SubsPlease answered {Request} with a non-JSON body (status {(int) Status})");

    public sealed record RequestFailed(string Request, Exception Exception) : ScrapeError(Exception.Message)
    {
        public override Action<ILogger> LogAction() => logger =>
            logger.LogError(Exception, "{domain_error.message} {domain_error.detail}", Message, $"Request failed for {Request}");
    }

    public sealed record UnusablePayload(string Request) : ScrapeError($"No usable payload for {Request}");
}
