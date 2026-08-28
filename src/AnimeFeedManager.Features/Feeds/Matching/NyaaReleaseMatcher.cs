using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Types;

namespace AnimeFeedManager.Features.Feeds.Matching;

public sealed record MatchedRelease(
    int SeriesId, string SeriesTitle, SeriesSeason Season, NyaaEntry Entry, ReleaseContent Content, bool IsBdRemux);

/// <summary>Ties title parsing and library matching together for one Nyaa entry.</summary>
internal static class NyaaReleaseMatcher
{
    public static MatchedRelease? Match(NyaaEntry entry, LibraryTitleIndex index)
    {
        var parsed = ReleaseTitleParser.Parse(entry.Title);
        var matched = index.TryMatch(parsed.CleanTitle);

        return matched is null
            ? null
            : new MatchedRelease(matched.MalId, matched.Title, matched.Season, entry, parsed.Content, parsed.IsBdRemux);
    }
}
