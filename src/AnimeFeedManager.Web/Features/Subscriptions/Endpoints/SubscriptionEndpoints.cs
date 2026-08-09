using AnimeFeedManager.Features.Feeds.Storage;
using AnimeFeedManager.Features.Subscriptions;
using AnimeFeedManager.Features.Subscriptions.Storage;
using AnimeFeedManager.Infrastructure.Cosmos;
using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Errors;
using AnimeFeedManager.Shared.Results.Static;
using AnimeFeedManager.Shared.Types;
using AnimeFeedManager.Web.Features.Catalog.Series;
using AnimeFeedManager.Web.Features.Security;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Subscriptions.Endpoints;

/// <summary>
/// Subscribe/unsubscribe toggle. htmx posts the <see cref="SubscribeButton"/>'s form here; the
/// handlers compose the Subscriptions domain logic (delegates built locally from
/// <see cref="ICosmosContainerFactory"/>), then re-render the button in its next state.
/// </summary>
internal static class SubscriptionEndpoints
{
    internal static IEndpointRouteBuilder MapSubscriptionEndpoints(this IEndpointRouteBuilder routes)
    {
        var subscriptions = routes.MapGroup("/subscriptions").RequireAuthorization();
        subscriptions.MapPost("/subscribe", Subscribe);
        subscriptions.MapPost("/unsubscribe", Unsubscribe);
        return routes;
    }

    private static Task<IResult> Subscribe(
        [FromForm] SubscribeForm form,
        HttpContext httpContext,
        ICosmosContainerFactory containerFactory,
        TimeProvider time,
        ILogger<SubscribeForm> logger,
        CancellationToken cancellationToken) =>
        ParseRequest(httpContext, form.Season)
            .Bind(parsed => SeriesSubscriptions.Subscribe(
                    parsed.UserId, form.SeriesId, parsed.Season,
                    containerFactory.CosmosUserSubscriptionUpserterHandler(),
                    containerFactory.CosmosSeriesSubscriberUpserterHandler(),
                    containerFactory.CosmosSubscriptionEventUpserterHandler(),
                    time, cancellationToken)
                .Map(_ => parsed.Season))
            .FlushLogs(logger)
            .MatchToValue<SeriesSeason, IResult>(
                season => RenderButton(form.SeriesId, season, form.Compact, isSubscribed: true),
                _ => RenderButton(form.SeriesId, FallbackSeason(form.Season), form.Compact, isSubscribed: false));

    private static Task<IResult> Unsubscribe(
        [FromForm] UnsubscribeForm form,
        HttpContext httpContext,
        ICosmosContainerFactory containerFactory,
        TimeProvider time,
        ILogger<UnsubscribeForm> logger,
        CancellationToken cancellationToken) =>
        ParseRequest(httpContext, form.Season)
            .Bind(parsed => SeriesSubscriptions.Unsubscribe(
                    parsed.UserId, form.SeriesId,
                    containerFactory.CosmosUserSubscriptionRemoverHandler(),
                    containerFactory.CosmosSeriesSubscriberRemoverHandler(),
                    containerFactory.CosmosSubscriptionEventUpserterHandler(),
                    time, cancellationToken)
                .Map(_ => parsed.Season))
            .FlushLogs(logger)
            .MatchToValue<SeriesSeason, IResult>(
                season => RenderButton(form.SeriesId, season, form.Compact, isSubscribed: false),
                _ => RenderButton(form.SeriesId, FallbackSeason(form.Season), form.Compact, isSubscribed: true));

    private static Result<(string UserId, SeriesSeason Season)> ParseRequest(HttpContext httpContext, string? season) =>
        CurrentUserId(httpContext)
            .Bind(userId => (season ?? string.Empty).ParseAsSeriesSeason()
                .Map(parsedSeason => (UserId: userId, Season: parsedSeason)));

    private static Result<string> CurrentUserId(HttpContext httpContext) =>
        httpContext.GetCurrentUser() switch
        {
            AuthenticatedUser user => Result<string>.Success(user.UserId),
            _ => Error.Create("Subscription action attempted without an authenticated user")
        };

    // Only reachable if Season was tampered with client-side (it's a hidden field the server
    // itself renders) — falls back to the sentinel so the button still has something to post
    // next time rather than failing to render at all.
    private static SeriesSeason FallbackSeason(string? season) =>
        (season ?? string.Empty).ParseAsSeriesSeason().MatchToValue(s => s, _ => SeriesSeason.Default);

    private static IResult RenderButton(int seriesId, SeriesSeason season, bool compact, bool isSubscribed) =>
        new RazorComponentResult<SubscribeButton>(new Dictionary<string, object?>
        {
            [nameof(SubscribeButton.SeriesId)] = seriesId,
            [nameof(SubscribeButton.Season)] = season,
            [nameof(SubscribeButton.Compact)] = compact,
            [nameof(SubscribeButton.IsSubscribed)] = isSubscribed
        });
}
