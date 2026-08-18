using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Admin;

/// <summary>
/// Shared inline SVGs for the admin trigger cards and their matching activity-row icons.
/// Sourced from the Claude Design admin mockup.
/// </summary>
internal static class AdminIcons
{
    private const string Attrs = "width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"";

    public static readonly MarkupString LatestSeason = new(
        $"<svg {Attrs}><path d=\"M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83\"/></svg>");

    public static readonly MarkupString CustomSeason = new(
        $"<svg {Attrs}><rect x=\"3\" y=\"5\" width=\"18\" height=\"16\" rx=\"2\"/><path d=\"M8 3v4M16 3v4M3 11h18\"/></svg>");

    public static readonly MarkupString TvReconciliation = new(
        $"<svg {Attrs}><path d=\"M4 11a9 9 0 0 1 9 9M4 4a16 16 0 0 1 16 16M5 19a1 1 0 1 1-2 0 1 1 0 0 1 2 0\"/></svg>");

    public static readonly MarkupString NonTvReconciliation = new(
        $"<svg {Attrs}><path d=\"M21 2v6h-6M3 12a9 9 0 0 1 15-6.7L21 8M3 22v-6h6M21 12a9 9 0 0 1-15 6.7L3 16\"/></svg>");

    public static readonly MarkupString AiringClockCheck = new(
        $"<svg {Attrs}><circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7v5l3 2\"/></svg>");

    // Activity-row-only icons — keyed by IPersistedEvent.Source, not a trigger card.
    public static readonly MarkupString LibraryImport = new(
        $"<svg {Attrs}><path d=\"M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4M7 10l5 5 5-5M12 15V3\"/></svg>");

    public static readonly MarkupString Classification = new(
        $"<svg {Attrs}><path d=\"M3 3h7v7H3zM14 3h7v7h-7zM14 14h7v7h-7zM3 14h7v7H3z\"/></svg>");
}
