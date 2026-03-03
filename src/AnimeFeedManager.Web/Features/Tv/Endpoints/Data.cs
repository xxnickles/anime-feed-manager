using System.Text.Json.Serialization;
using AnimeFeedManager.Shared.Types;
using AnimeFeedManager.Web.Features.Tv.Controls;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal sealed record RemoveSeriesEvent(string Owner);

internal sealed record RemoveSeriesTrigger(RemoveSeriesEvent RemoveSeries);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RemoveSeriesTrigger))]
internal partial class TvEndpointJsonContext : JsonSerializerContext;

internal static class Data
{
    internal static Result<(AuthenticatedUser User, TvInterestedViewModel Model)> AddUser(HttpContext context,
        TvInterestedViewModel viewModel)
    {
        var user = context.GetCurrentUser();
        return user switch
        {
            AuthenticatedUser au => (au, viewModel),
            _ => Error.Create("User can not be anonymous")
        };
    }
    
    
    internal static Result<(AuthenticatedUser User, TvSubscriptionViewModel Model)> AddUser(HttpContext context,
        TvSubscriptionViewModel viewModel)
    {
        var user = context.GetCurrentUser();
        return user switch
        {
            AuthenticatedUser au => (au, viewModel),
            _ => Error.Create("User can not be anonymous")
        };
    }
}