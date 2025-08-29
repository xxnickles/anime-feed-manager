namespace AnimeFeedManager.Web.Htmx;

/// <summary>
/// Captures a redirect response and sets the "HX-Redirect" header for an HTMX request
/// This is a workaround for the fact that HTMX does not handle redirects automatically and just present the "redirected" content 
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
            // Check if the response is a redirect (302 or 303) and the request is an HTMX request
            if (context.Response.StatusCode is not (302 or 303) ||
                requestType is not (HxBoosted or HxForm)) return Task.CompletedTask;
            // Set the "HX-Redirect" header to the location of the redirect
            var location = context.Response.Headers["Location"].ToString();
            context.Response.Headers["HX-Redirect"] = location;
            context.Response.StatusCode = 200;
            return Task.CompletedTask;
        });
        await _next(context);
    }
}