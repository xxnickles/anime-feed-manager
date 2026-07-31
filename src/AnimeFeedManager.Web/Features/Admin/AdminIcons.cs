using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Admin;

/// <summary>
/// Shared inline SVGs for the admin trigger cards and (later) their matching activity-row
/// icons — one icon per operation, reused wherever that operation shows up so it stays
/// recognizable across the page. Sourced from the Claude Design admin mockup.
/// </summary>
internal static class AdminIcons
{
    private const string Attrs = "width=\"18\" height=\"18\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"";

    public static readonly MarkupString LatestSeason = new(
        $"<svg {Attrs}><path d=\"M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83\"/></svg>");

    public static readonly MarkupString CustomSeason = new(
        $"<svg {Attrs}><rect x=\"3\" y=\"5\" width=\"18\" height=\"16\" rx=\"2\"/><path d=\"M8 3v4M16 3v4M3 11h18\"/></svg>");

    public static readonly MarkupString NyaaCollection = new(
        $"<svg {Attrs}><path d=\"M4 11a9 9 0 0 1 9 9M4 4a16 16 0 0 1 16 16M5 19a1 1 0 1 1-2 0 1 1 0 0 1 2 0\"/></svg>");

    public static readonly MarkupString NyaaReconciliation = new(
        $"<svg {Attrs}><path d=\"M21 2v6h-6M3 12a9 9 0 0 1 15-6.7L21 8M3 22v-6h6M21 12a9 9 0 0 1-15 6.7L3 16\"/></svg>");

    public static readonly MarkupString AiringClockCheck = new(
        $"<svg {Attrs}><circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7v5l3 2\"/></svg>");
}
