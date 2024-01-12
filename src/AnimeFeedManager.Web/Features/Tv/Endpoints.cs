using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Web.Features.Common;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
using AnimeFeedManager.Web.Features.Common.TvControls;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Tv;

public static class Endpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/tv/subscribe", (
                [FromForm] AvailableTvSeriesControlData data,
                [FromServices] BlazorRenderer renderer,
                [FromServices] IAddTvSubscription tvSubscriber,
                [FromServices] ILogger<TvGrid> logger,
                CancellationToken token) =>
            Validate(data)
                .BindAsync(info => tvSubscriber.Subscribe(info.UserId, info.Series, token))
                .MapAsync(_ => renderer.RenderSubscribedControls(data))
                .ToComponentResult(renderer, logger, $"Subscription to {data.Title} has been created",
                    renderer.RenderUnSubscribedControls(data))
        ).RequireAuthorization();


        app.MapPost("/tv/unsubscribe", (
                [FromForm] AvailableTvSeriesControlData data,
                [FromServices] BlazorRenderer renderer,
                [FromServices] IRemoveTvSubscription tvUnSubscriber,
                [FromServices] ILogger<TvGrid> logger,
                CancellationToken token) =>
            Validate(data)
                .BindAsync(info => tvUnSubscriber.Unsubscribe(info.UserId, info.Series, token))
                .MapAsync(_ => renderer.RenderUnSubscribedControls(data))
                .ToComponentResult(renderer, logger, $"Subscription to {data.Title} has been removed",
                    renderer.RenderSubscribedControls(data))
        ).RequireAuthorization();


        app.MapPost("/tv/add-interested", (
                [FromForm] NotAvailableControlData data,
                [FromServices] BlazorRenderer renderer,
                [FromServices] IAddInterested tvInterestedSubscriber,
                [FromServices] ILogger<TvGrid> logger,
                CancellationToken token) =>
            Validate(data)
                .BindAsync(info => tvInterestedSubscriber.Add(info.UserId, info.Series, token))
                .MapAsync(_ => renderer.RenderInterestedControls(data))
                .ToComponentResult(renderer, logger, $"{data.Title} has been added to your interest list",
                    renderer.RenderNotAvailableControls(data))
        ).RequireAuthorization();


        app.MapPost("/tv/remove-interested", (
                [FromForm] NotAvailableControlData data,
                [FromServices] BlazorRenderer renderer,
                [FromServices] IRemoveInterestedSeries tvInterestedRemover,
                [FromServices] ILogger<TvGrid> logger,
                CancellationToken token) =>
            Validate(data)
                .BindAsync(info => tvInterestedRemover.Remove(info.UserId, info.Series, token))
                .MapAsync(_ => renderer.RenderNotAvailableControls(data))
                .ToComponentResult(renderer, logger, $"{data.Title} has been added to your interest list",
                    renderer.RenderInterestedControls(data))
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