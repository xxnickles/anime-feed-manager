namespace AnimeFeedManager.Old.Common.Types;

public enum ProcessScope
{
    TvSubscriptions,
    TvInterested,
    OvasSubscriptions,
    MoviesSubscriptions
}

public readonly record struct ProcessResult(ushort Completed, ProcessScope Scope);