namespace AnimeFeedManager.Web.Features.Common;
using AttributeDefinition = (string DataAttribute, string Attribute, object Value) ;
internal static class Attributes
{
    internal static AttributeDefinition AvailableAttributeDefinition = ("data-available", "available", "true");
    internal static AttributeDefinition InterestedAttributeDefinition = ("data-interested", "interested", "true");
    internal static AttributeDefinition CompletedAttributeDefinition = ("data-completed", "completed", "true");
    internal static AttributeDefinition SubscribedAttributeDefinition = ("data-subscribed", "subscribed", "true");
    internal static AttributeDefinition HasFeedAttributeDefinition = ("data-has-feed", "hasFeed", "true");
    
    
    internal static readonly KeyValuePair<string, object> Available = new(AvailableAttributeDefinition.DataAttribute, AvailableAttributeDefinition.Value);
    internal static readonly KeyValuePair<string, object> Interested = new(InterestedAttributeDefinition.DataAttribute, InterestedAttributeDefinition.Value);
    internal static readonly KeyValuePair<string, object> Completed = new(CompletedAttributeDefinition.DataAttribute, CompletedAttributeDefinition.Value);
    internal static readonly KeyValuePair<string, object> Subscribed = new(SubscribedAttributeDefinition.DataAttribute, SubscribedAttributeDefinition.Value);
    internal static readonly KeyValuePair<string, object> HasFeed = new(HasFeedAttributeDefinition.DataAttribute, HasFeedAttributeDefinition.Value);
}