using System.Text.Json.Serialization;

namespace AnimeFeedManager.Web.Htmx;

internal abstract record HtmxRequestType;

/// <summary>A normal browser navigation — not an HTMX request. Server returns the full document.</summary>
internal sealed record Html : HtmxRequestType;

/// <summary>An API request (<c>Accept: application/json</c>).</summary>
internal sealed record Json : HtmxRequestType;

/// <summary>
/// Base for any HTMX-initiated request.
/// <para><see cref="Boosted"/> reflects the <c>HX-Boosted</c> header — orthogonal to full/partial
/// in HTMX 4 (a boosted link can still target a specific element). <see cref="CurrentPagePath"/>
/// is the path+query from <c>HX-Current-URL</c> (root <c>/</c> when absent).</para>
/// </summary>
internal abstract record Htmx(bool Boosted, string CurrentPagePath) : HtmxRequestType;

/// <summary>
/// A "full" HTMX request (<c>HX-Request-Type: full</c>): the swap targets <c>&lt;body&gt;</c> or uses
/// <c>hx-select</c> — covers standard <c>hx-boost</c> navigations and history restores. Server returns
/// the full HTML document; HTMX swaps the body / merges the head / selects the fragment client-side.
/// </summary>
internal sealed record HxFull(bool Boosted, string CurrentPagePath) : Htmx(Boosted, CurrentPagePath);

/// <summary>
/// A "partial" HTMX request (<c>HX-Request-Type: partial</c>): a targeted swap into a specific,
/// non-body element. Server returns only the fragment that replaces the target.
/// </summary>
internal sealed record HxPartial(bool Boosted, string CurrentPagePath) : Htmx(Boosted, CurrentPagePath);

internal static class HtmxExtensions
{
    public static HtmxRequestType GetHtmxRequestType(this IHttpContextAccessor context)
    {
        return context.HttpContext?.Features.Get<HtmxRequestFeature>()?.RequestType ?? new Html();
    }
}

/// <summary>
/// Options for the HX-Location response header when using the JSON object form.
/// Tells HTMX to perform an AJAX navigation with fine-grained control over the swap.
/// </summary>
internal sealed record HxLocationOptions(
    string Path,
    string? Target = null,
    string? Swap = null,
    string? Select = null,
    Dictionary<string, string>? Headers = null,
    Dictionary<string, object?>? Values = null);

/// <summary>
/// Source-generated JSON serialization context for HTMX types.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(HxLocationOptions))]
internal partial class HtmxJsonContext : JsonSerializerContext;
