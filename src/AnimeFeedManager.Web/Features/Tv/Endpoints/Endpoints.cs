namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

public static class Endpoints
{
    public static void MapTvInterestedEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/tv/add-interested", InterestedHandlers.AddSeriesToInterested)
            .RequireAuthorization();
        
        group.MapPost("/tv/remove-interested", InterestedHandlers.RemoveInterestedSeries)
            .RequireAuthorization();
    }
}