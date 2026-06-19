namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Weekly library import: every Saturday at 04:00 UTC, runs an import for the current season.
/// Delegates to the shared <see cref="LibraryImportJob"/> composition root with
/// <see cref="ImportTarget.Now()"/>; the manual admin triggers call the same seam with their own
/// target. The cron expression is overridable via configuration (<see cref="CronJobOverride"/>);
/// SkipIfRunning is inherited <c>true</c> so a long-running prior fire is skipped rather than
/// overlapping (the executor's per-job gate, keyed <c>"library-import"</c>).
/// </summary>
internal sealed class LibraryImportCronJob(LibraryImportJob job) : CronJob
{
    public override string Name => "library-import";

    public override string DefaultExpression => "0 4 * * 6";

    public override Task Run(CancellationToken cancellationToken) =>
        job.Run(ImportTarget.Now(), cancellationToken);
}
