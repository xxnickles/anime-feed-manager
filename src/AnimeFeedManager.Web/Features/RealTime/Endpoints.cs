using Microsoft.AspNetCore.Http.Connections;

namespace AnimeFeedManager.Web.Features.RealTime;

internal static class Endpoints
{
    /// <summary>Hub base the SignalR client connects to; it posts to <c>{Hub}/negotiate</c>.</summary>
    internal const string Hub = "/realtime";

    private static readonly ActivitySource Source = new(Telemetry.WebRealTimeSource);

    internal static void MapRealTimeEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost($"{Hub}/negotiate", Negotiate);
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
            .MatchToValue<NegotiationResponse, IResult>(
                negotiation => Results.Json(new NegotiateResponse(negotiation.Url, negotiation.AccessToken)),
                _ => Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
    }

    private sealed record NegotiateResponse(string? Url, string? AccessToken);
}
