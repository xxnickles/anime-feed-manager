namespace AnimeFeedManager.Features.Library.Types;

/// <summary>
/// Original Net Animation — streaming-native release. May have a
/// weekly <see cref="Broadcast"/> (e.g., simulcast slot) or be batch-dropped.
/// </summary>
public sealed record OnaSeries : Series
{
    public OnaSeries(int malId) : base(malId)
    {
    }

    public Broadcast? Broadcast { get; init; }
    public int? Episodes { get; init; }
    public int? EpisodeDurationMinutes { get; init; }
}