using AnimeFeedManager.Features.Feeds.Collection;
using AnimeFeedManager.Infrastructure.Background.Jobs;
using AnimeFeedManager.Web.Features.Components;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

/// <summary>
/// Admin Nyaa-reconciliation and airing-clock triggers. Bodiless htmx-posted <c>&lt;form&gt;</c>s that
/// fire the TV path (<see cref="TvReconciliationJob"/>), non-airing path (<see cref="NonAiringReconciliationJob"/>),
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

        nyaa.MapPost("/tv-reconciliation", TriggerTvReconciliation);
        nyaa.MapPost("/non-airing-reconciliation", TriggerNonAiringReconciliation);

        routes.MapPost("/feeds/airing-clock-check", TriggerAiringClockCheck);

        return routes;
    }

    private static IResult TriggerTvReconciliation([FromForm] Noop _, JobExecutor executor)
    {
        executor.Trigger<TvReconciliationJob>(
            "tv-reconciliation",
            (job, ct) => job.Run(ct));

        return Toasts.Success(
            "TV reconciliation",
            Toasts.Text("TV reconciliation run started — running in the background."));
    }

    private static IResult TriggerNonAiringReconciliation([FromForm] Noop _, JobExecutor executor)
    {
        executor.Trigger<NonAiringReconciliationJob>(
            "non-airing-reconciliation",
            (job, ct) => job.Run(ct));

        return Toasts.Success(
            "Non-airing reconciliation",
            Toasts.Text("Non-airing reconciliation run started — running in the background."));
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
