namespace AnimeFeedManager.Features.Feeds.Matching;

/// <summary>
/// The numbering shape of a parsed release — closed so consumers pattern-match instead of
/// null-checking. Orthogonal to whether a release is a BD/remux (see
/// <see cref="ParsedRelease.IsBdRemux"/>): a BD release can itself be a single episode, a batch,
/// or non-numbered (a movie).
/// </summary>
public abstract record ReleaseContent
{
    private ReleaseContent()
    {
    }

    public sealed record SingleEpisode(int Number) : ReleaseContent;

    public sealed record Batch(int Start, int End) : ReleaseContent;

    public sealed record NonNumbered : ReleaseContent;
}
