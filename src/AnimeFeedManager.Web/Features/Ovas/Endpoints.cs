using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
using AnimeFeedManager.Web.Features.Ovas.Controls;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Ovas;

public static class Endpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/ovas/subscribe", (
                [FromForm] OvaControlData data,
                [FromServices] IAddOvasSubscription ovaSubscriber,
                [FromServices] ILogger<OvasGrid> logger,
                CancellationToken token
            ) =>
            Validate(data)
                .BindAsync(validatedData => ovaSubscriber.Subscribe(validatedData.UserId,
                    validatedData.SeriesId, validatedData.NotificationDate, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkSubscribedResponse(data, $"{data.Title} has been added to your Ova subscriptions"),
                    e => ComponentResponses.ErrorResponse(data, e, logger))
        );

        app.MapPost("/ovas/unsubscribe", (
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