using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
using AnimeFeedManager.Web.Features.Movies.Controls;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Movies;

public static class Endpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/movies/subscribe", (
                [FromForm] MovieControlData data,
                [FromServices] IAddMovieSubscription movieSubscriber,
                [FromServices] ILogger<MoviesGrid> logger,
                CancellationToken token
            ) =>
            Validate(data)
                .BindAsync(validatedData => movieSubscriber.Subscribe(validatedData.UserId,
                    validatedData.SeriesId, validatedData.NotificationDate, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkSubscribedResponse(data, $"{data.Title} has been added to your Movie subscriptions"),
                    e => ComponentResponses.ErrorResponse(data, e, logger))
        );

        app.MapPost("/movies/unsubscribe", (
                [FromForm] MovieControlData data,
                [FromServices] IRemoveMovieSubscription movieUnSubscriber,
                [FromServices] ILogger<MoviesGrid> logger,
                CancellationToken token
            ) =>
            Validate(data).BindAsync(validatedData => movieUnSubscriber
                    .Unsubscribe(validatedData.UserId,
                        validatedData.SeriesId, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkUnsubscribedResponse(data,
                        $"{data.Title} has been removed from your Movie subscriptions"),
                    e => ComponentResponses.ErrorResponse(data, e, logger))
        );
    }


    private static Either<DomainError, (UserId UserId, RowKey SeriesId, DateTime NotificationDate)>
        Validate(
            MovieControlData payload)
    {
        return (
                UserId.Validate(payload.UserId),
                RowKey.Validate(payload.Title)
            ).Apply((userid, series) => (userid, series, payload.NotificationTime))
            .ValidationToEither();
    }
}