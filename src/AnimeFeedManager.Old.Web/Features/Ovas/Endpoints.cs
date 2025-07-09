using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Validators;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Infrastructure.Messaging;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.IO;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.Types;
using AnimeFeedManager.Old.Web.Features.Common;
using AnimeFeedManager.Old.Web.Features.Common.DefaultResponses;
using AnimeFeedManager.Old.Web.Features.Ovas.Controls;
using AnimeFeedManager.Old.Web.Features.Security;
using Microsoft.AspNetCore.Mvc;
using RowKey = AnimeFeedManager.Old.Common.Types.RowKey;
using SeasonInformation = AnimeFeedManager.Old.Common.Types.SeasonInformation;

namespace AnimeFeedManager.Old.Web.Features.Ovas;

public static class Endpoints
{
    public static void MapOvaEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/ovas/subscribe", (
                [FromForm] OvaControlData data,
                [FromServices] IOvasSubscriptionStore ovaSubscriber,
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
                    [FromForm] SeriesToUpdate removeInfo,
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
                            { SeasonInfo = new SeasonInformation(safeData.Season.Season, safeData.Season.Year) }))
                .ToComponentResult(
                    data => ComponentResponses.OkResponse(data.SeasonInfo,
                        $"{removeInfo.Title} has been removed from the ovas library"),
                    error => CommonComponentResponses.ErrorComponentResult(error, logger)))
            .RequireAuthorization(Policies.AdminRequired);
        
        group.MapPost("/ovas/remove-feed", (
                    [FromForm] SeriesToUpdate feedUpdateInfo,
                    [FromServices] IOvaFeedRemover feedRemover,
                    [FromServices] IDomainPostman domainPostman,
                    [FromServices] ILogger<OvasGrid> logger,
                    CancellationToken token) =>
                (PartitionKey.Validate(feedUpdateInfo.Season), RowKey.Validate(feedUpdateInfo.Id), SeasonValidators.ValidateSeasonPartitionString(feedUpdateInfo.Season))
                .Apply((key, rowKey, season) => new { PartitionKey = key, RowKey = rowKey, Season = season })
                .ValidationToEither()
                .BindAsync(safeData =>
                    feedRemover.RemoveFeed(safeData.RowKey, safeData.PartitionKey, token)
                        .MapAsync(_ => new
                            { SeasonInfo = new SeasonInformation(safeData.Season.Season, safeData.Season.Year) }))
                .BindAsync(data =>
                    domainPostman.SendMessage(new OvaFeedRemovedEvent(feedUpdateInfo.Title), token)
                        .MapAsync(_ => data))
                .ToComponentResult(
                    data => ComponentResponses.OkResponse(data.SeasonInfo,
                        $"{feedUpdateInfo.Title} feed information has been removed"),
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