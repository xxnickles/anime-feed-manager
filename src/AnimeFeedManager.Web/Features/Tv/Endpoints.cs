using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Web.Features.Common;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
using AnimeFeedManager.Web.Features.Security;
using AnimeFeedManager.Web.Features.Tv.Controls;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Tv;

public static class Endpoints
{
    public static void MapTvEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/tv/subscribe", (
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


        group.MapPost("/tv/unsubscribe", (
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


        group.MapPost("/tv/add-interested", (
                [FromForm] NotAvailableControlData data,
                [FromServices] IAddInterested tvInterestedSubscriber,
                [FromServices] ILogger<TvGrid> logger,
                CancellationToken token) =>
            ValidateSubscription(data)
                .BindAsync(info => tvInterestedSubscriber.Add(info.UserId, info.SeriesId, info.Series, token))
                .ToComponentResult(
                    _ => ComponentResponses.OkResponse<InterestedAnimeControls>(data,
                        $"{data.Title} has been added to your interest list"),
                    e => ComponentResponses.ErrorResponse<NotAvailableAnimeControls>(data, e, logger)
                )
        ).RequireAuthorization();

        group.MapPut("/tv/alternative-title", (
                    [FromForm] AlternativeTitleUpdate updateInfo,
                    [FromServices] IDomainPostman domainPostman,
                    [FromServices] ILogger<TvGrid> logger,
                    CancellationToken token) =>
                domainPostman.SendMessage(
                        new UpdateAlternativeTitle(updateInfo.Id, updateInfo.Season, updateInfo.Title,
                            updateInfo.OriginalTitle, updateInfo.Status), token)
                    .ToComponentResult("Alternative title update will be processed in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);


        group.MapPost("/tv/remove", (
                    [FromForm] SeriesToRemove removeInfo,
                    [FromServices] ITvSeriesStore tvSeriesStore,
                    [FromServices] ILogger<TvGrid> logger,
                    CancellationToken token) =>
                (PartitionKey.Validate(removeInfo.Season), RowKey.Validate(removeInfo.Id),
                    SeasonValidators.ValidateSeasonPartitionString(removeInfo.Season))
                .Apply((key, rowKey, season) => new {PartitionKey = key, RowKey = rowKey, Season = season})
                .ValidationToEither()
                .BindAsync(safeData =>
                    tvSeriesStore.RemoveSeries(safeData.RowKey, safeData.PartitionKey, token)
                        .MapAsync(_ => new
                            {SeasonInfo = new SeasonInformation(safeData.Season.Season, safeData.Season.Year)}))
                .ToComponentResult(
                    data => ComponentResponses.OkResponse(data.SeasonInfo,
                        $"{removeInfo.Title} has been removed from the library"),
                    error => CommonComponentResponses.ErrorComponentResult(error, logger)))
            .RequireAuthorization(Policies.AdminRequired);


        group.MapPost("/tv/remove-interested", (
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
                UserId.Validate(payload.UserId),
                NoEmptyString.FromString(payload.FeedId)
                    .ToValidation(ValidationError.Create("FeedId", ["Series cannot be en empty string"]))
            ).Apply((userid, series) => (userid, series))
            .ValidationToEither();
    }

    private static Either<DomainError, (UserId UserId, NoEmptyString Series)> Validate(NotAvailableControlData payload)
    {
        return (
                UserId.Validate(payload.UserId),
                NoEmptyString.FromString(payload.Title)
                    .ToValidation(ValidationError.Create("FeedId", ["Series cannot be en empty string"]))
            ).Apply((userid, series) => (userid, series))
            .ValidationToEither();
    }

    private static Either<DomainError, (UserId UserId, RowKey SeriesId, NoEmptyString Series)> ValidateSubscription(
        NotAvailableControlData payload)
    {
        return (
                UserId.Validate(payload.UserId),
                RowKey.Validate(payload.SeriesId),
                NoEmptyString.FromString(payload.Title)
                    .ToValidation(ValidationError.Create("FeedId", ["Series cannot be en empty string"]))
            ).Apply((userid, seriesId, series) => (userid, seriesId, series))
            .ValidationToEither();
    }
}