namespace AnimeFeedManager.Features.Library.Import;

/// <summary>
/// Weekly library import: every Saturday at 04:00 UTC, enqueues an import for the
/// current season. The cron expression is overridable via configuration; see
/// <see cref="CronJobOverride"/>. SkipIfRunning is inherited as <c>true</c> so a
/// long-running prior fire blocks the next one rather than overlapping.
/// </summary>
internal sealed class LibraryImportCronJob(WorkQueue<LibraryImportCommand> queue) : CronJob
{
    public override string Name => "library-import";

    public override string DefaultExpression => "0 4 * * 6";

    public override Task Run(CancellationToken cancellationToken)
        => queue.Enqueue(new LibraryImportCommand(ImportTarget.Now()), cancellationToken).AsTask();
}
