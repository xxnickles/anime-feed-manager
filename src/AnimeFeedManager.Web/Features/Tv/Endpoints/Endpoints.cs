namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

public static class Endpoints
{
    public static void MapTvSubscriptionsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/tv/add-interested", InterestedHandlers.AddSeriesToInterested)
            .RequireAuthorization();
        
        group.MapPost("/tv/remove-interested", InterestedHandlers.RemoveInterestedSeries)
            .RequireAuthorization();
        
        group.MapPost("/tv/subscribe", SubscriptionHandlers.Subscribe)
            .RequireAuthorization();
        
        group.MapPost("/tv/unsubscribe",SubscriptionHandlers.Unsubscribe)
            .RequireAuthorization();
        
        group.MapPost("/tv/alternative-title",LibraryManagement.UpdateAlternativeSeries)
            .RequireAuthorization(Policies.AdminRequired);
    }
}