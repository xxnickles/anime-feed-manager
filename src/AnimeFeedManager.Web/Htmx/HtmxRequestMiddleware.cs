namespace AnimeFeedManager.Web.Htmx;

internal sealed class HtmxRequestFeature
{
    internal HtmxRequestType RequestType { get; }

    internal HtmxRequestFeature(HtmxRequestType requestType)
    {
        RequestType = requestType;
    }
}


/// <summary>
/// Custom middleware to add HTMX feature to the request
/// </summary>
internal sealed class HtmxRequestMiddleware
{
    private readonly RequestDelegate _next;

    public HtmxRequestMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var htmxRequestType = GetHtmxRequestType(context);
        var htmxRequestFeature = new HtmxRequestFeature(htmxRequestType);
        context.Features.Set(htmxRequestFeature);

        await _next(context);
    }

    internal static HtmxRequestType GetHtmxRequestType(HttpContext context)
    {
        // Check if the request is a JSON request ...
        if (context.Request.Headers.TryGetValue("Accept", out var acceptHeader) &&
            acceptHeader.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            return HtmxRequestType.Json;
        }

        //...or if the request is an HTMX request
        if (context.Request.Headers.TryGetValue("HX-Request", out _))
        {
            return context.Request.Headers.TryGetValue("HX-Boosted", out _)
                ? HtmxRequestType.HxBoosted
                : HtmxRequestType.HxForm;
        }

        // Default to HTML
        return HtmxRequestType.Html;
    }
}