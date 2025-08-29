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