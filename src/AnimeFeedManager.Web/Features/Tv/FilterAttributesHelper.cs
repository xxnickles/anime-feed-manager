using AnimeFeedManager.Features.Tv.Library.Queries;

namespace AnimeFeedManager.Web.Features.Tv;

public static class FilterAttributesHelper
{
    public static Dictionary<string, bool> GetFilterAttributes(UserTvSeries series) => series switch
    {
        Subscribed => new Dictionary<string, bool> { ["available"] = true, ["subscribed"] = true },
        Available or AvailableForSubscription => new Dictionary<string, bool> { ["available"] = true },
        Interested => new Dictionary<string, bool> { ["interested"] = true },
        Completed => new Dictionary<string, bool> { ["completed"] = true },
        _ => new Dictionary<string, bool> { ["not-available"] = true }
    };
}
