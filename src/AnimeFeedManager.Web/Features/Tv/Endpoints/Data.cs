using System.Text.Json.Serialization;
using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using AnimeFeedManager.Shared.Types;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal sealed record RemoveSeriesEvent(string Owner);

internal sealed record RemoveSeriesTrigger(RemoveSeriesEvent RemoveSeries);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RemoveSeriesTrigger))]
internal partial class TvEndpointJsonContext : JsonSerializerContext;

internal static class Data
{
    // Season comes from the id, so the series read stays a partition + row key point operation.
    internal static Task<Result<(AuthenticatedUser User, TvSeries Series)>> GetSeriesForUser(
        HttpContext context,
        string seriesId,
        TvLibrarySeries seriesGetter,
        Uri publicBlobUri,
        CancellationToken token) =>
        CurrentUser(context)
            .Bind(user => IdHelpers.SeriesSeasonFromId(seriesId).Map(season => (User: user, Season: season)))
            .Bind(data => seriesGetter(seriesId, data.Season, publicBlobUri, token)
                .Map(series => (data.User, Series: series)));

    private static Result<AuthenticatedUser> CurrentUser(HttpContext context) =>
        context.GetCurrentUser() switch
        {
            AuthenticatedUser user => user,
            _ => Error.Create("User can not be anonymous")
        };
}
