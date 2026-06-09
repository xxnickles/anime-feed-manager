using AnimeFeedManager.Web.Htmx.Static;

namespace AnimeFeedManager.Web.Htmx;

/// <summary>
/// Captures a redirect response and sets the "HX-Location" header for an HTMX request.
/// HX-Location tells HTMX to do an AJAX navigation (SPA-like) instead of a full page reload.
/// </summary>
/// <remarks>
/// HTMX (including v4) never sees a 3xx response — the browser fetch/XHR layer follows redirects
/// transparently — and HTMX 4 explicitly does not process response headers on 3xx status codes.
/// So we intercept the 302/303, emit HX-Location, and rewrite the status to 200 so the client
/// performs an AJAX navigation instead.
/// </remarks>
public sealed class HtmxRedirectResponseMiddleware
{
    private readonly RequestDelegate _next;

    public HtmxRedirectResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var requestType = context.Features.Get<HtmxRequestFeature>()?.RequestType ?? new Html();
            if (context.Response.StatusCode is not (302 or 303) ||
                requestType is not Htmx) return Task.CompletedTask;

            var location = context.Response.Headers["Location"].ToString();
            context.Response.HxLocation(location);
            context.Response.StatusCode = 200;
            return Task.CompletedTask;
        });
        await _next(context);
    }
}
