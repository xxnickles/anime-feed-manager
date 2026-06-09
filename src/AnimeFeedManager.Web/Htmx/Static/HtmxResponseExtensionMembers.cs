using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AnimeFeedManager.Web.Htmx.Static;

/// <summary>
/// Typed extension methods for setting HTMX response headers.
/// These headers instruct the HTMX client how to handle the server response
/// (navigation, DOM swapping, event triggering, title updates, etc.).
/// </summary>
/// <remarks>
/// HTMX 4 note: response headers are NOT processed on 3xx status codes — always return a 2xx
/// when setting any of these headers. The HTMX 2 headers HX-Trigger-After-Swap and
/// HX-Trigger-After-Settle were removed in HTMX 4; use <see cref="HxTrigger(HttpResponse, string)"/>
/// (or client-side JS listening to <c>htmx:after:settle</c>) instead.
/// </remarks>
internal static class HtmxResponseExtensionMembers
{

    // ── Navigation / History ──────────────────────────────────────────

    /// <summary>
    /// Performs a full browser redirect (not AJAX). The page will do a traditional
    /// navigation to the given URL, bypassing HTMX swap logic entirely.
    /// </summary>
    public static void HxRedirect(this HttpResponse response, string url)
        => response.Headers["HX-Redirect"] = url;

    /// <summary>
    /// Performs an AJAX-style navigation to the given URL.
    /// HTMX will fetch the URL via AJAX and swap the content into the page.
    /// </summary>
    public static void HxLocation(this HttpResponse response, string url)
        => response.Headers["HX-Location"] = url;

    /// <summary>
    /// Performs an AJAX-style navigation with fine-grained control over
    /// the target element, swap strategy, content selection, and request parameters.
    /// </summary>
    public static void HxLocation(this HttpResponse response, HxLocationOptions options)
        => response.Headers["HX-Location"] = JsonSerializer.Serialize(options, HtmxJsonContext.Default.HxLocationOptions);

    /// <summary>
    /// Pushes a new URL into the browser's history stack, updating the address bar.
    /// Does not trigger a navigation — only changes the visible URL.
    /// </summary>
    public static void HxPushUrl(this HttpResponse response, string url)
        => response.Headers["HX-Push-Url"] = url;

    /// <summary>
    /// Suppresses the default URL push that HTMX would normally perform.
    /// Only <c>false</c> has an effect; passing <c>true</c> is a no-op
    /// because a URL string is required for an actual push.
    /// </summary>
    public static void HxPushUrl(this HttpResponse response, bool enabled)
    {
        if (!enabled) response.Headers["HX-Push-Url"] = "false";
    }

    /// <summary>
    /// Replaces the current URL in the browser's history without adding a new entry.
    /// Useful for normalizing URLs after form submissions.
    /// </summary>
    public static void HxReplaceUrl(this HttpResponse response, string url)
        => response.Headers["HX-Replace-Url"] = url;

    /// <summary>
    /// Suppresses the default URL replacement that HTMX would normally perform.
    /// Only <c>false</c> has an effect; passing <c>true</c> is a no-op
    /// because a URL string is required for an actual replacement.
    /// </summary>
    public static void HxReplaceUrl(this HttpResponse response, bool enabled)
    {
        if (!enabled) response.Headers["HX-Replace-Url"] = "false";
    }

    // ── Swap Control ──────────────────────────────────────────────────

    /// <summary>
    /// Overrides the swap target element for this response.
    /// HTMX will swap the response content into the element matching the CSS selector
    /// instead of the original triggering element's target.
    /// </summary>
    public static void HxRetarget(this HttpResponse response, string cssSelector)
        => response.Headers["HX-Retarget"] = cssSelector;

    /// <summary>
    /// Overrides the swap strategy for this response.
    /// Accepts the same syntax as the hx-swap attribute. HTMX 4 swap styles:
    /// innerHTML, outerHTML, innerMorph, outerMorph, textContent, beforebegin,
    /// afterbegin, beforeend, afterend, delete, none — plus modifiers like transition:true,
    /// swap:500ms, and the split scroll syntax (scroll:top scrollTarget:#id / show:top showTarget:#id).
    /// </summary>
    public static void HxReswap(this HttpResponse response, string swapStrategy)
        => response.Headers["HX-Reswap"] = swapStrategy;

    /// <summary>
    /// Selects a subset of the response HTML to swap in, using a CSS selector.
    /// Only the matched fragment will be used for the swap; the rest is discarded.
    /// </summary>
    public static void HxReselect(this HttpResponse response, string cssSelector)
        => response.Headers["HX-Reselect"] = cssSelector;

    // ── Page State ────────────────────────────────────────────────────

    /// <summary>
    /// Forces a full page refresh on the client. The browser will reload the
    /// current page as if the user pressed F5.
    /// </summary>
    public static void HxRefresh(this HttpResponse response)
        => response.Headers["HX-Refresh"] = "true";

    // ── Event Triggering ──────────────────────────────────────────────

    /// <summary>
    /// Triggers a client-side event immediately when the response is received
    /// (before the DOM swap). Listeners can use <c>htmx:trigger</c> or
    /// standard DOM event listeners to react.
    /// </summary>
    public static void HxTrigger(this HttpResponse response, string eventName)
        => response.Headers["HX-Trigger"] = eventName;

    /// <summary>
    /// Triggers one or more client-side events with detail data immediately
    /// when the response is received (before the DOM swap).
    /// Each dictionary entry becomes an event; the value is accessible via <c>evt.detail</c>.
    /// Callers provide a <see cref="JsonTypeInfo{T}"/> from a source-generated context.
    /// </summary>
    public static void HxTrigger<T>(this HttpResponse response, T events, JsonTypeInfo<T> typeInfo)
        => response.Headers["HX-Trigger"] = JsonSerializer.Serialize(events, typeInfo);
}
