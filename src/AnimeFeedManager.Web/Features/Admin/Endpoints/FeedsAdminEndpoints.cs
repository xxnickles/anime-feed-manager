using AnimeFeedManager.Features.Feeds.Collection;
using AnimeFeedManager.Infrastructure.Background.Jobs;
using AnimeFeedManager.Web.Features.Components;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

/// <summary>
/// Admin Nyaa-collection and airing-clock triggers. Bodiless htmx-posted <c>&lt;form&gt;</c>s that
/// fire the TV path (<see cref="TvReconciliationJob"/>), non-TV path (<see cref="NonTvReconciliationJob"/>),
/// and airing-clock (<see cref="AiringClockCheckJob"/>) runs in-process via <see cref="JobExecutor"/>,
/// sharing the same single-flight gate keys as their cron wrappers so manual and scheduled runs stay
/// mutually exclusive. Feedback is an immediate OOB toast; a second, stats-bearing toast follows
/// asynchronously over SSE if the run matches/flags anything (see <c>FeedsNotifications</c>). Nests
/// under the caller's shared <c>/admin</c> group (auth applied once there) rather than building its own.
/// </summary>
internal static class FeedsAdminEndpoints
{
    internal static IEndpointRouteBuilder MapFeedsAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var nyaa = routes.MapGroup("/feeds/nyaa");

        nyaa.MapPost("/collection", TriggerCollection);
        nyaa.MapPost("/reconciliation", TriggerReconciliation);

        routes.MapPost("/feeds/airing-clock-check", TriggerAiringClockCheck);

        return routes;
    }

    private static IResult TriggerCollection([FromForm] Noop _, JobExecutor executor)
    {
        executor.Trigger<TvReconciliationJob>(
            "nyaa-collection",
            (job, ct) => job.Run(ct));

        return Toasts.Success(
            "Nyaa collection",
            Toasts.Text("Hot-path collection run started — running in the background."));
    }

    private static IResult TriggerReconciliation([FromForm] Noop _, JobExecutor executor)
    {
        executor.Trigger<NonTvReconciliationJob>(
            "nyaa-reconciliation",
            (job, ct) => job.Run(ct));

        return Toasts.Success(
            "Nyaa reconciliation",
            Toasts.Text("Cold-path reconciliation run started — running in the background."));
    }

    private static IResult TriggerAiringClockCheck([FromForm] Noop _, JobExecutor executor)
    {
        executor.Trigger<AiringClockCheckJob>(
            "airing-clock-check",
            (job, ct) => job.Run(ct));

        return Toasts.Success(
            "Airing clock check",
            Toasts.Text("Airing-clock run started — running in the background."));
    }
}
