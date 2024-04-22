using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.IO;
using AnimeFeedManager.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Web.Features.Common;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
using AnimeFeedManager.Web.Features.Ovas.Controls;
using AnimeFeedManager.Web.Features.Security;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Ovas;

public static class Endpoints
{
    public static void MapOvaEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/ovas/subscribe", (
                [FromForm] OvaControlData data,
                [FromServices] IAddOvasSubscription ovaSubscriber,
                [FromServices] ILogger<OvasGrid> logger,
                CancellationToken token
            ) =>
            Validate(data)
                .BindAsync(validatedData => ovaSubscriber.Subscribe(validatedData.UserId,
                    validatedData.SeriesId, validatedData.NotificationDate, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkSubscribedResponse(data,
                        $"{data.Title} has been added to your Ova subscriptions"),
                    e => ComponentResponses.ErrorResponse(data, e, logger))
        );

        group.MapPost("/ovas/unsubscribe", (
                [FromForm] OvaControlData data,
                [FromServices] IRemoveOvasSubscription ovaUnSubscriber,
                [FromServices] ILogger<OvasGrid> logger,
                CancellationToken token
            ) =>
            Validate(data).BindAsync(validatedData => ovaUnSubscriber
                    .Unsubscribe(validatedData.UserId,
                        validatedData.SeriesId, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkUnsubscribedResponse(data,
                        $"{data.Title} has been removed from your Ova subscriptions"),
                    e => ComponentResponses.ErrorResponse(data, e, logger))
        );

        group.MapPost("/ovas/remove", (
                    [FromForm] SeriesToRemove removeInfo,
                    [FromServices] IOvasStorage ovasStorage,
                    [FromServices] ILogger<OvasGrid> logger,
                    CancellationToken token) =>
                (PartitionKey.Validate(removeInfo.Season), RowKey.Validate(removeInfo.Id),
                    SeasonValidators.ValidateSeasonPartitionString(removeInfo.Season))
                .Apply((key, rowKey, season) => new { PartitionKey = key, RowKey = rowKey, Season = season })
                .ValidationToEither()
                .BindAsync(safeData =>
                    ovasStorage.RemoveOva(safeData.RowKey, safeData.PartitionKey, token)
                        .MapAsync(_ => new
                            { SeasonInfo = new SeasonInformation(safeData.Season.season, safeData.Season.year) }))
                .ToComponentResult(
                    data => ComponentResponses.OkResponse(data.SeasonInfo,
                        $"{removeInfo.Title} has been removed from the ovas library"),
                    error => CommonComponentResponses.ErrorComponentResult(error, logger)))
            .RequireAuthorization(Policies.AdminRequired);
    }


    private static Either<DomainError, (UserId UserId, RowKey SeriesId, DateTime NotificationDate)>
        Validate(
            OvaControlData payload)
    {
        return (
                UserId.Validate(payload.UserId),
                RowKey.Validate(payload.Title)
            ).Apply((userid, series) => (userid, series, payload.NotificationTime))
            .ValidationToEither();
    }
}