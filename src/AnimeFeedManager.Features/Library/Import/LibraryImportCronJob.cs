namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Weekly library import: every Saturday at 04:00 UTC, runs an import for the current
/// season. The cron expression is overridable via configuration; see
/// <see cref="CronJobOverride"/>. SkipIfRunning is inherited as <c>true</c> so a
/// long-running prior fire is skipped rather than overlapping (the executor's per-job gate).
/// </summary>
internal sealed class LibraryImportCronJob(LibraryImport import) : CronJob
{
    public override string Name => "library-import";

    public override string DefaultExpression => "0 4 * * 6";

    public override Task Run(CancellationToken cancellationToken)
        => import.Execute(ImportTarget.Now(), cancellationToken);
}
