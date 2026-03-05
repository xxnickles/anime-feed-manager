namespace AnimeFeedManager.Web.Features.Components.SeriesGrid;

public readonly record struct FilterCounters(
    uint Total, 
    uint Available,
    uint Interested,
    uint Subscribed,
    uint Completed, 
    uint NotAvailable);