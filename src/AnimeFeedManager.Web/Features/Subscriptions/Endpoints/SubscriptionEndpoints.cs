using AnimeFeedManager.Features.Feeds.Storage;
using AnimeFeedManager.Features.Subscriptions;
using AnimeFeedManager.Features.Subscriptions.Storage;
using AnimeFeedManager.Web.Features.Catalog.Series;
using AnimeFeedManager.Web.Features.Components;
using AnimeFeedManager.Web.Features.Components.Responses;
using AnimeFeedManager.Web.Features.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Subscriptions.Endpoints;

/// <summary>
/// Subscribe/unsubscribe toggle. htmx posts the <see cref="SubscribeButton"/>'s form here; the
/// handlers compose the Subscriptions domain logic (delegates built locally from
/// <see cref="ICosmosContainerFactory"/>), then respond with the button's next-state render plus
/// a toast — <see cref="ComponentResults.ToComponentResult{T}"/> aggregates both into one response.
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

    private static Task<RazorComponentResult> Subscribe(
        [FromForm] SubscribeForm form,
        HttpContext httpContext,
        ICosmosContainerFactory containerFactory,
        TimeProvider time,
        ILogger<SubscribeForm> logger,
        CancellationToken cancellationToken) =>
        ParseRequest(httpContext, form.Season)
            .Bind(parsed => SeriesSubscriptions.Subscribe(
                    parsed.UserId, form.SeriesId, parsed.Season,
                    containerFactory.CosmosUserSubscriptionUpserter(),
                    containerFactory.CosmosSeriesSubscriberUpserterHandler(),
                    containerFactory.CosmosSubscriptionEventUpserterHandler(),
                    time, cancellationToken)
                .Map(_ => parsed.Season))
            .FlushLogs(logger)
            .ToComponentResult(
                season =>
                [
                    ButtonFragment(form.SeriesId, season, form.Compact, form.SeriesTitle, isSubscribed: true),
                    Toasts.SuccessFragment("Subscribe", Toasts.Text("You're now subscribed."))
                ],
                error =>
                [
                    ButtonFragment(form.SeriesId, FallbackSeason(form.Season), form.Compact, form.SeriesTitle, isSubscribed: false),
                    Toasts.ErrorFragment("Subscribe", error)
                ]);

    private static Task<RazorComponentResult> Unsubscribe(
        [FromForm] UnsubscribeForm form,
        HttpContext httpContext,
        ICosmosContainerFactory containerFactory,
        TimeProvider time,
        ILogger<UnsubscribeForm> logger,
        CancellationToken cancellationToken) =>
        ParseRequest(httpContext, form.Season)
            .Bind(parsed => SeriesSubscriptions.Unsubscribe(
                    parsed.UserId, form.SeriesId,
                    containerFactory.CosmosUserSubscriptionRemover(),
                    containerFactory.CosmosSeriesSubscriberRemoverHandler(),
                    containerFactory.CosmosSubscriptionEventUpserterHandler(),
                    time, cancellationToken)
                .Map(_ => parsed.Season))
            .FlushLogs(logger)
            .ToComponentResult(
                season =>
                [
                    ButtonFragment(form.SeriesId, season, form.Compact, form.SeriesTitle, isSubscribed: false),
                    Toasts.SuccessFragment("Unsubscribe", Toasts.Text("You've been unsubscribed."))
                ],
                error =>
                [
                    ButtonFragment(form.SeriesId, FallbackSeason(form.Season), form.Compact, form.SeriesTitle, isSubscribed: true),
                    Toasts.ErrorFragment("Unsubscribe", error)
                ]);

    private static Result<(NoEmptyString UserId, SeriesSeason Season)> ParseRequest(HttpContext httpContext, string? season) =>
        CurrentUserId(httpContext)
            .Bind(userId => (season ?? string.Empty).ParseAsSeriesSeason()
                .Map(parsedSeason => (UserId: userId, Season: parsedSeason)));

    private static Result<NoEmptyString> CurrentUserId(HttpContext httpContext) =>
        httpContext.GetCurrentUser() switch
        {
            AuthenticatedUser user => Result<NoEmptyString>.Success(user.UserId),
            _ => Error.Create("Subscription action attempted without an authenticated user")
        };

    // Only reachable if Season was tampered with client-side (it's a hidden field the server
    // itself renders) — falls back to the sentinel so the button still has something to post
    // next time rather than failing to render at all.
    private static SeriesSeason FallbackSeason(string? season) =>
        (season ?? string.Empty).ParseAsSeriesSeason().MatchToValue(s => s, _ => SeriesSeason.Default);

    private static RenderFragment ButtonFragment(int seriesId, SeriesSeason season, bool compact, string? seriesTitle, bool isSubscribed) =>
        ComponentResponseHelpers.AsFragment<SubscribeButton>(new Dictionary<string, object?>
        {
            [nameof(SubscribeButton.SeriesId)] = seriesId,
            [nameof(SubscribeButton.Season)] = season,
            [nameof(SubscribeButton.Compact)] = compact,
            [nameof(SubscribeButton.SeriesTitle)] = seriesTitle ?? string.Empty,
            [nameof(SubscribeButton.IsSubscribed)] = isSubscribed
        });
}
