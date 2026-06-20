using AnimeFeedManager.Features.Library.Import;
using AnimeFeedManager.Infrastructure.Background.Jobs;
using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Errors;
using AnimeFeedManager.Shared.Results.Static;
using AnimeFeedManager.Shared.Types;
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
        import.MapPost("/season", TriggerSeason);

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

    // Same fire-and-forget seam as the latest trigger, pinned to a chosen season. The form
    // already passed client validation, but the client is bypassable — so we re-validate
    // through the domain. The channel split: field-shaped failures (DomainValidationErrors)
    // land back in the form's field slots via OOB swaps; the import outcome (or an unexpected
    // error) shows as a toast. The browser keeps the user's entries (hx-swap="none").
    private static IResult TriggerSeason([FromForm] SeasonImportForm form, JobExecutor executor) =>
        ParseYear(form.Year)
            .Bind(year => (form.Season ?? string.Empty, year).ParseAsSeriesSeason())
            .MatchToValue(
                season =>
                {
                    executor.Trigger<LibraryImportJob>(
                        "library-import",
                        (job, ct) => job.Run(ImportTarget.For(season), ct));

                    return Notifications.Success(
                        "Library import",
                        Notifications.Text($"{season} import started — running in the background."));
                },
                error => error is DomainValidationErrors validation
                    ? FieldErrors.Oob(validation)
                    : Notifications.Error("Library import", error));

    // Lenient year bind: a missing / non-numeric value becomes a "Year"-tagged domain error
    // (matching the field slot) instead of a raw 400. Range validation stays in Year.ParseAsYear,
    // reached once the value is a number via the (season, year) tuple parse above.
    private static Result<int> ParseYear(string? raw) =>
        int.TryParse(raw, out var year)
            ? year
            : DomainValidationError.Create<Year>($"'{raw}' is not a valid year").ToErrors();
}
