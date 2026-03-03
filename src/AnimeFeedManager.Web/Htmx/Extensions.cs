using System.Text.Json.Serialization;

namespace AnimeFeedManager.Web.Htmx;

internal abstract record HtmxRequestType;

internal sealed record Html : HtmxRequestType;

internal sealed record Json : HtmxRequestType;

internal sealed record HxBoosted : HtmxRequestType;

internal sealed record HxForm(string CurrentPagePath) : HtmxRequestType;

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
