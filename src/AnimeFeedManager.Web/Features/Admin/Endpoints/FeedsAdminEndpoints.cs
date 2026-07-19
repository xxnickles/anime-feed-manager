using AnimeFeedManager.Features.Feeds.Collection;
using AnimeFeedManager.Infrastructure.Background.Jobs;
using AnimeFeedManager.Web.Features.Components;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

/// <summary>
/// Admin Nyaa-collection triggers. Bodiless htmx-posted <c>&lt;form&gt;</c>s that fire the hot-path
/// (<see cref="NyaaCollectionJob"/>) and cold-path (<see cref="NyaaReconciliationJob"/>) collection
/// runs in-process via <see cref="JobExecutor"/>, sharing the same single-flight gate keys as their
/// cron wrappers so manual and scheduled runs stay mutually exclusive. Feedback is an immediate OOB
/// toast; a second, stats-bearing toast follows asynchronously over SSE if the run matches anything
/// (see <c>FeedsNotifications</c>). Nests under the caller's shared <c>/admin</c> group (auth
/// applied once there) rather than building its own.
/// </summary>
internal static class FeedsAdminEndpoints
{
    internal static IEndpointRouteBuilder MapFeedsAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var nyaa = routes.MapGroup("/feeds/nyaa");

        nyaa.MapPost("/collection", TriggerCollection);
        nyaa.MapPost("/reconciliation", TriggerReconciliation);

        return routes;
    }

    private static IResult TriggerCollection([FromForm] Noop _, JobExecutor executor)
    {
        executor.Trigger<NyaaCollectionJob>(
            "nyaa-collection",
            (job, ct) => job.Run(ct));

        return Toasts.Success(
            "Nyaa collection",
            Toasts.Text("Hot-path collection run started — running in the background."));
    }

    private static IResult TriggerReconciliation([FromForm] Noop _, JobExecutor executor)
    {
        executor.Trigger<NyaaReconciliationJob>(
            "nyaa-reconciliation",
            (job, ct) => job.Run(ct));

        return Toasts.Success(
            "Nyaa reconciliation",
            Toasts.Text("Cold-path reconciliation run started — running in the background."));
    }
}
