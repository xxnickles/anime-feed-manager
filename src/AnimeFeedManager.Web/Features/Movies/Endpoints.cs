using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Movies.Scrapping.Series.IO;
using AnimeFeedManager.Features.Movies.Subscriptions.IO;
using AnimeFeedManager.Features.Movies.Subscriptions.Types;
using AnimeFeedManager.Web.Features.Common;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
using AnimeFeedManager.Web.Features.Movies.Controls;
using AnimeFeedManager.Web.Features.Security;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Movies;

public static class Endpoints
{
    public static void MapMovieEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/movies/subscribe", (
                [FromForm] MovieControlData data,
                [FromServices] IMovieSubscriptionStore movieSubscriber,
                [FromServices] ILogger<MoviesGrid> logger,
                CancellationToken token
            ) =>
            Validate(data)
                .BindAsync(validatedData => movieSubscriber.Subscribe(validatedData.UserId,
                    validatedData.SeriesId, validatedData.NotificationDate, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkSubscribedResponse(data,
                        $"{data.Title} has been added to your Movie subscriptions"),
                    e => ComponentResponses.ErrorResponse(data, e, logger))
        );

        group.MapPost("/movies/unsubscribe", (
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

        group.MapPost("/movies/remove", (
                    [FromForm] SeriesToUpdate removeInfo,
                    [FromServices] IMoviesStorage moviesStorage,
                    [FromServices] ILogger<MoviesGrid> logger,
                    CancellationToken token) =>
                (PartitionKey.Validate(removeInfo.Season), RowKey.Validate(removeInfo.Id),
                    SeasonValidators.ValidateSeasonPartitionString(removeInfo.Season))
                .Apply((key, rowKey, season) => new {PartitionKey = key, RowKey = rowKey, Season = season})
                .ValidationToEither()
                .BindAsync(safeData =>
                    moviesStorage.RemoveMovie(safeData.RowKey, safeData.PartitionKey, token)
                        .MapAsync(_ => new
                            {SeasonInfo = new SeasonInformation(safeData.Season.Season, safeData.Season.Year)}))
                .ToComponentResult(
                    data => ComponentResponses.OkResponse(data.SeasonInfo,
                        $"{removeInfo.Title} has been removed from the movie library"),
                    error => CommonComponentResponses.ErrorComponentResult(error, logger)))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPost("/movies/remove-feed", (
                    [FromForm] SeriesToUpdate feedUpdateInfo,
                    [FromServices] IMovieFeedRemover feedRemover,
                    [FromServices] IDomainPostman domainPostman,
                    [FromServices] ILogger<MoviesGrid> logger,
                    CancellationToken token) =>
                (PartitionKey.Validate(feedUpdateInfo.Season), RowKey.Validate(feedUpdateInfo.Id),
                    SeasonValidators.ValidateSeasonPartitionString(feedUpdateInfo.Season))
                .Apply((key, rowKey, season) => new {PartitionKey = key, RowKey = rowKey, Season = season})
                .ValidationToEither()
                .BindAsync(safeData =>
                    feedRemover.RemoveFeed(safeData.RowKey, safeData.PartitionKey, token)
                        .MapAsync(_ => new
                            {SeasonInfo = new SeasonInformation(safeData.Season.Season, safeData.Season.Year)}))
                .BindAsync(data =>
                    domainPostman.SendMessage(new MovieFeedRemovedEvent(feedUpdateInfo.Title), token)
                        .MapAsync(_ => data))
                .ToComponentResult(
                    data => ComponentResponses.OkResponse(data.SeasonInfo,
                        $"{feedUpdateInfo.Title} feed information has been removed"),
                    error => CommonComponentResponses.ErrorComponentResult(error, logger)))
            .RequireAuthorization(Policies.AdminRequired);
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