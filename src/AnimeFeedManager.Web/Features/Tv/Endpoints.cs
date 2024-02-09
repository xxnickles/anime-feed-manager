using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
using AnimeFeedManager.Web.Features.Tv.Controls;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Tv;

public static class Endpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/tv/subscribe", (
                [FromForm] AvailableTvSeriesControlData data,
                [FromServices] IAddTvSubscription tvSubscriber,
                [FromServices] ILogger<TvGrid> logger,
                CancellationToken token) =>
            Validate(data)
                .BindAsync(info => tvSubscriber.Subscribe(info.UserId, info.Series, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkResponse<SubscribedAnimeControls>(data,
                        $"Subscription to {data.Title} has been created"),
                    e => ComponentResponses.ErrorResponse<UnsubscribedAnimeControls>(data, e, logger))
        ).RequireAuthorization();


        app.MapPost("/tv/unsubscribe", (
                [FromForm] AvailableTvSeriesControlData data,
                [FromServices] IRemoveTvSubscription tvUnSubscriber,
                [FromServices] ILogger<TvGrid> logger,
                CancellationToken token) =>
            Validate(data)
                .BindAsync(info => tvUnSubscriber.Unsubscribe(info.UserId, info.Series, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkResponse<UnsubscribedAnimeControls>(data,
                        $"Subscription to {data.Title} has been removed"),
                    e => ComponentResponses.ErrorResponse<SubscribedAnimeControls>(data, e, logger)
                )
        ).RequireAuthorization();


        app.MapPost("/tv/add-interested", (
                [FromForm] NotAvailableControlData data,
                [FromServices] IAddInterested tvInterestedSubscriber,
                [FromServices] ILogger<TvGrid> logger,
                CancellationToken token) =>
            Validate(data)
                .BindAsync(info => tvInterestedSubscriber.Add(info.UserId, info.Series, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkResponse<InterestedAnimeControls>(data,
                        $"{data.Title} has been added to your interest list"),
                    e => ComponentResponses.ErrorResponse<NotAvailableAnimeControls>(data, e, logger)
                )
        ).RequireAuthorization();


        app.MapPost("/tv/remove-interested", (
                [FromForm] NotAvailableControlData data,
                [FromServices] IRemoveInterestedSeries tvInterestedRemover,
                [FromServices] ILogger<TvGrid> logger,
                CancellationToken token) =>
            Validate(data)
                .BindAsync(info => tvInterestedRemover.Remove(info.UserId, info.Series, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkResponse<NotAvailableAnimeControls>(data,
                        $"{data.Title} has been removed to your interest list"),
                    e => ComponentResponses.ErrorResponse<InterestedAnimeControls>(data, e, logger)
                )
        ).RequireAuthorization();
    }

    private static Either<DomainError, (UserId UserId, NoEmptyString Series)> Validate(
        AvailableTvSeriesControlData payload)
    {
        return (
                UserIdValidator.Validate(payload.UserId),
                NoEmptyString.FromString(payload.FeedId)
                    .ToValidation(ValidationError.Create("FeedId", ["Series cannot be en empty string"]))
            ).Apply((userid, series) => (userid, series))
            .ValidationToEither();
    }

    private static Either<DomainError, (UserId UserId, NoEmptyString Series)> Validate(
        NotAvailableControlData payload)
    {
        return (
                UserIdValidator.Validate(payload.UserId),
                NoEmptyString.FromString(payload.Title)
                    .ToValidation(ValidationError.Create("FeedId", ["Series cannot be en empty string"]))
            ).Apply((userid, series) => (userid, series))
            .ValidationToEither();
    }
}