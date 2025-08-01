namespace AnimeFeedManager.Web.Htmx;

internal enum HtmxRequestType
{
    Html,
    Json,
    HxBoosted,
    HxForm
}

internal static class HtmxExtensions
{
    public static HtmxRequestType GetHtmxRequestType(this IHttpContextAccessor context)
    {
        return context.HttpContext?.Features.Get<HtmxRequestFeature>()?.RequestType ?? HtmxRequestType.Html;
    }
}