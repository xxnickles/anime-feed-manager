namespace AnimeFeedManager.Features.Library.Types;

/// <summary>
/// Theatrical anime film — single release date, single total runtime.
/// </summary>
public sealed record MovieSeries : Series
{
    public MovieSeries(int malId) : base(malId)
    {
    }

    public int? RuntimeMinutes { get; init; }
}