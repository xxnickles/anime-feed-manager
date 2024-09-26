namespace AnimeFeedManager.Web.Features.Common;

internal static class Attributes
{
    internal static readonly KeyValuePair<string, object> Available = new("data-available", "true");
    internal static readonly KeyValuePair<string, object> Susbcribed = new("data-subscribed", "true");
    internal static readonly KeyValuePair<string, object> HasFeed = new("data-has-feed", "true");
}