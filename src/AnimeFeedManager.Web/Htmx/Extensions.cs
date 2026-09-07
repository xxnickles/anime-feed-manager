using System.Text.Json.Serialization;

namespace AnimeFeedManager.Web.Htmx;

internal abstract record HtmxRequestType;

internal sealed record Html : HtmxRequestType;

internal sealed record Json : HtmxRequestType;

/// <summary>
/// A boosted page navigation. Renders identically to <see cref="Html"/> (full document) under the
/// whole-body-boost shell model; kept as its own type for non-rendering consumers (redirect handling,
/// future request tagging), not for render branching.
/// </summary>
internal sealed record HxBoosted : HtmxRequestType;

/// <summary>
/// A non-boosted htmx request (explicit hx-get/hx-post + hx-target) expecting a targeted fragment back.
/// </summary>
internal sealed record Partial(string CurrentPagePath) : HtmxRequestType;

internal static class HtmxExtensions
{
    public static HtmxRequestType GetHtmxRequestType(this IHttpContextAccessor context)
    {
        return context.HttpContext?.Features.Get<HtmxRequestFeature>()?.RequestType ?? new Html();
    }

    /// <summary>
    /// True when this request was classified as an htmx request (<see cref="HxBoosted"/> or
    /// <see cref="Partial"/>) by <see cref="HtmxRequestMiddleware"/>. Requires <c>UseHtmx()</c> to have
    /// run earlier in the pipeline.
    /// </summary>
    public static bool IsHtmxRequest(this HttpContext context) =>
        context.Features.Get<HtmxRequestFeature>()?.RequestType is HxBoosted or Partial;
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
