using System.Collections.Immutable;
using AnimeFeedManager.Features.Tv.Library.Queries;
using AnimeFeedManager.Shared.Types;
using AnimeFeedManager.Web.Features.Components.SeriesGrid;

namespace AnimeFeedManager.Web.Features.Tv;

public static class FilterAttributesHelper
{
    private const string AvailableKey = "available";
    private const string CompletedKey = "completed";
    private const string NotAvailableKey = "not-available";
    private const string SubscribedKey = "subscribed";
    private const string InterestedKey = "interested";

    // Airing state and subscription state are independent facets — a series can be both
    // completed and subscribed — so each contributes its own attribute.
    public static Dictionary<string, bool> GetFilterAttributes(UserTvSeries series)
    {
        var attributes = AiringAttribute(series.TvSeries.Status);

        switch (series)
        {
            case Subscribed or Completed { IsSubscribed: true }:
                attributes[SubscribedKey] = true;
                break;
            case Interested:
                attributes[InterestedKey] = true;
                break;
        }

        return attributes;
    }

    // Counted from the same attributes the grid filters on, so counts always match what a filter shows.
    public static FilterCounters GetCounters(ImmutableArray<UserTvSeries> series)
    {
        var facets = series.Select(GetFilterAttributes).ToArray();
        return new FilterCounters(
            Total: (uint) series.Length,
            Available: Count(facets, AvailableKey),
            Interested: Count(facets, InterestedKey),
            Subscribed: Count(facets, SubscribedKey),
            Completed: Count(facets, CompletedKey),
            NotAvailable: Count(facets, NotAvailableKey));
    }

    private static uint Count(Dictionary<string, bool>[] facets, string key) =>
        (uint) facets.Count(facet => facet.GetValueOrDefault(key));

    private static Dictionary<string, bool> AiringAttribute(SeriesStatus status) => status.ToString() switch
    {
        SeriesStatus.OngoingValue => new Dictionary<string, bool> { [AvailableKey] = true },
        SeriesStatus.CompletedValue => new Dictionary<string, bool> { [CompletedKey] = true },
        _ => new Dictionary<string, bool> { [NotAvailableKey] = true }
    };
}
