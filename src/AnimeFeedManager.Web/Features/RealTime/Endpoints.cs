using Microsoft.AspNetCore.Http.Connections;

namespace AnimeFeedManager.Web.Features.RealTime;

internal static class Endpoints
{
    /// <summary>Hub base the SignalR client connects to; it posts to <c>{Hub}/negotiate</c>.</summary>
    internal const string Hub = "/realtime";

    private static readonly ActivitySource Source = new(Telemetry.WebRealTimeSource);

    internal static void MapRealTimeEndpoints(this RouteGroupBuilder group)
    {
        // The SignalR client can't use an HTML status page, and re-executing the token-less POST fails antiforgery as 400.
        group.MapPost($"{Hub}/negotiate", Negotiate)
            .WithMetadata(new SkipStatusCodePagesAttribute());
    }

    private static async Task<IResult> Negotiate(
        HttpContext context,
        NotificationsHub hub,
        ILogger<NotificationsHub> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(RealTimeTelemetry.NegotiateSpan);
        var user = context.GetCurrentUser();
        return await hub.GetContext()
            .Bind(hubContext => Negotiation.Negotiate(hubContext, user, cancellationToken))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .MatchToValue<NegotiationResponse, RealTimeError, IResult>(
                negotiation => Results.Json(new NegotiateResponse(negotiation.Url, negotiation.AccessToken)),
                _ => Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
    }

    private sealed record NegotiateResponse(string? Url, string? AccessToken);
}
