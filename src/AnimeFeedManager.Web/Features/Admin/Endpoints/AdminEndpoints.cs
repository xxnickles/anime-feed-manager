using AnimeFeedManager.Features.Library.Import;
using AnimeFeedManager.Infrastructure.Background.Jobs;
using AnimeFeedManager.Web.Features.Components;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

/// <summary>
/// Admin season-library triggers. htmx-posted <c>&lt;form&gt;</c>s that fire an in-process import
/// via the shared <see cref="LibraryImportJob"/> seam and return a Razor card fragment as
/// feedback. Antiforgery stays on: the cards post a form carrying the token, and the
/// <c>[FromForm]</c> binding makes each endpoint enforce validation.
/// </summary>
internal static class AdminEndpoints
{
    internal static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var import = routes.MapGroup("/admin/library/import");

        import.MapPost("/latest", TriggerLatest);

        return routes;
    }

    // Fire-and-forget under the "library-import" single-flight gate (shared with the weekly
    // cron, so only one import runs at a time); feedback is an OOB toast (the card isn't
    // re-rendered — the trigger uses hx-swap="none").
    private static IResult TriggerLatest([FromForm] Noop _, JobExecutor executor)
    {
        executor.Trigger<LibraryImportJob>(
            "library-import",
            (job, ct) => job.Run(ImportTarget.Now(), ct));

        return Notifications.Success(
            "Library import",
            Notifications.Text("Latest season import started — running in the background."));
    }
}
