using AnimeFeedManager.Features.Tv.Library.Queries;
using AnimeFeedManager.Shared.Types;

namespace AnimeFeedManager.Web.Features.Tv;

public static class FilterAttributesHelper
{
    // Airing state and subscription state are independent facets — a series can be both
    // completed and subscribed — so each contributes its own attribute.
    public static Dictionary<string, bool> GetFilterAttributes(UserTvSeries series)
    {
        var attributes = AiringAttribute(series.TvSeries.Status);

        switch (series)
        {
            case Subscribed:
                attributes["subscribed"] = true;
                break;
            case Interested:
                attributes["interested"] = true;
                break;
        }

        return attributes;
    }

    private static Dictionary<string, bool> AiringAttribute(SeriesStatus status) => status.ToString() switch
    {
        SeriesStatus.OngoingValue => new Dictionary<string, bool> { ["available"] = true },
        SeriesStatus.CompletedValue => new Dictionary<string, bool> { ["completed"] = true },
        _ => new Dictionary<string, bool> { ["not-available"] = true }
    };
}
