namespace AnimeFeedManager.Features.Library.Types;

/// <summary>
/// Original Video Animation — direct-to-video/Blu-ray episodic release.
/// No broadcast schedule; volumes drop over an air range.
/// </summary>
public sealed record OvaSeries : Series
{
    public OvaSeries(int malId) : base(malId)
    {
    }

    public int? Episodes { get; init; }
    public int? EpisodeDurationMinutes { get; init; }
}