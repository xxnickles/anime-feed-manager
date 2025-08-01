namespace AnimeFeedManager.Web.Htmx;

internal static class Registration
{
    /// <summary>
    /// Register the HTMX middleware. Use it as one of the first middlewares in the pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseHtmx(this IApplicationBuilder app)
    {
        app.UseMiddleware<HtmxRequestMiddleware>();
        app.UseMiddleware<HtmxRedirectResponseMiddleware>();
        return app;
    }
}