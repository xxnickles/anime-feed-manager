namespace AnimeFeedManager.Web.Htmx;

/// <summary>
/// Captures a redirect response and sets the "HX-Location" header for an HTMX request.
/// HX-Location tells HTMX to do an AJAX navigation (SPA-like) instead of a full page reload.
/// </summary>
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
                requestType is not (HxBoosted or HxForm)) return Task.CompletedTask;

            var location = context.Response.Headers["Location"].ToString();
            context.Response.Headers["HX-Location"] = location;
            context.Response.StatusCode = 200;
            return Task.CompletedTask;
        });
        await _next(context);
    }
}