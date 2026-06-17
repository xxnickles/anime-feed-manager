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
        var headers = context.Request.Headers;

        // Check if the request is a JSON request ...
        if (headers.TryGetValue("Accept", out var acceptHeader) &&
            acceptHeader.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            return new Json();
        }

        //...or if the request is an HTMX request
        if (headers.TryGetValue("HX-Request", out _))
        {
            // htmx 4 Back/Forward re-fetches the URL (HX-History-Restore-Request) and swaps
            // <body>, expecting a FULL document — not a boosted #main-content fragment. Render
            // the full document so the shell is rebuilt correctly for the restored URL; the
            // view transition on the restore swap comes from htmx.config.transitions.
            if (headers.ContainsKey("HX-History-Restore-Request"))
                return new Html();

            var boosted = headers.ContainsKey("HX-Boosted");
            var currentPage = GetCurrentPageUrl(context); // single value expected, otherwise we assign the root path

            // HTMX 4 reports full-vs-partial authoritatively via HX-Request-Type ("full" | "partial").
            // It's sent on every HTMX 4 request; if it's ever absent, fall back to the HTMX 2 heuristic
            // (boosted navigations swap the body, so they're "full").
            var requestType = headers["HX-Request-Type"].ToString();
            var isFull = string.IsNullOrEmpty(requestType) ? boosted : requestType == "full";

            return isFull
                ? new HxFull(boosted, currentPage)
                : new HxPartial(boosted, currentPage);
        }

        // Default to HTML
        return new Html();
    }

    private static string GetCurrentPageUrl(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("HX-Current-URL", out var url) && url.FirstOrDefault() is { } currentUrl)
        {
            // if the headers come, it is url
            return new Uri(currentUrl).PathAndQuery;
        }

        return "/";
    }
}
