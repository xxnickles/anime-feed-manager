using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Types;

namespace AnimeFeedManager.Features.Feeds.Matching;

public sealed record MatchedRelease(int SeriesId, NyaaEntry Entry, ReleaseContent Content, bool IsBdRemux);

/// <summary>Ties title parsing and library matching together for one Nyaa entry.</summary>
internal static class NyaaReleaseMatcher
{
    public static MatchedRelease? Match(NyaaEntry entry, LibraryTitleIndex index)
    {
        var parsed = ReleaseTitleParser.Parse(entry.Title);
        return index.TryMatch(parsed.CleanTitle, out var seriesId)
            ? new MatchedRelease(seriesId, entry, parsed.Content, parsed.IsBdRemux)
            : null;
    }
}
