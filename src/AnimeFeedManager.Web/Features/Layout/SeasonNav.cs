using System.Collections.Immutable;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons.Types;

namespace AnimeFeedManager.Web.Features.Layout;

// Shared projection for the season-nav surfaces (topbar pills + mobile drawer): the most-recent
// seasons, newest first. Both surfaces read the same index doc and render their own markup.
internal static class SeasonNav
{
    public const int RecentCount = 4;

    // Most-recent seasons, newest first. The drawer renders this directly (newest on top); the
    // topbar pills render it reversed (oldest -> newest, current on the right).
    public static ImmutableArray<SeasonEntry> Recent(LibrarySeasonsIndex index) =>
        index.Seasons.IsDefaultOrEmpty
            ? []
            : [.. index.Seasons.OrderByDescending(entry => entry.SeriesSeason).Take(RecentCount)];

    // The season the landing (/) shows — the airing marker if set, else the newest — so the nav
    // entry that links to "/" highlights there and matches what the landing resolves to.
    public static SeasonEntry? Current(ImmutableArray<SeasonEntry> recent) =>
        recent.IsDefaultOrEmpty ? null : recent.FirstOrDefault(entry => entry.IsCurrent) ?? recent[0];
}
