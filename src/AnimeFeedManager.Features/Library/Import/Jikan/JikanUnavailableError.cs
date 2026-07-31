namespace AnimeFeedManager.Features.Library.Import.Jikan;

/// <summary>
/// Jikan returned 504 (BadResponseException) — a known, recurring occurrence across its endpoints
/// (confirmed by manual reproduction, independent of query strings), not a transient blip worth
/// retrying. Callers recover it explicitly to an empty/degraded result rather than failing outright.
/// </summary>
public sealed record JikanUnavailableError : DomainError
{
    public string Endpoint { get; }

    private JikanUnavailableError(string endpoint, string message) : base(message)
    {
        Endpoint = endpoint;
    }

    public static JikanUnavailableError Create(string endpoint) =>
        new(endpoint, $"Jikan reported unavailable (504) for {endpoint}");

    public override Action<ILogger> LogAction() => logger => logger.LogWarning("{Message}", Message);
}
