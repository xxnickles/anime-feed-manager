using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Types;

namespace AnimeFeedManager.Features.Feeds.Matching;

public sealed record MatchedRelease(int SeriesId, string SeriesTitle, NyaaEntry Entry, ReleaseContent Content, bool IsBdRemux);

/// <summary>Ties title parsing and library matching together for one Nyaa entry.</summary>
internal static class NyaaReleaseMatcher
{
    public static MatchedRelease? Match(NyaaEntry entry, LibraryTitleIndex index)
    {
        var parsed = ReleaseTitleParser.Parse(entry.Title);
        if (!index.TryMatch(parsed.CleanTitle, out var seriesId)) return null;

        // Falls back to the parsed release title only if the index somehow has no title for an
        // id it just matched — shouldn't happen, since the id came from this same index.
        var seriesTitle = index.GetTitle(seriesId) ?? parsed.CleanTitle;
        return new MatchedRelease(seriesId, seriesTitle, entry, parsed.Content, parsed.IsBdRemux);
    }
}
