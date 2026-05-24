namespace AnimeFeedManager.Infrastructure.Background.Cron;

/// <summary>
/// Options bound from the top-level <c>"CronJobs"</c> JSON array. Inherits from
/// <see cref="List{T}"/> so the configuration binder populates entries directly
/// without an intermediate wrapper property — keeping the JSON flat:
/// <c>"CronJobs": [ { "Name": ..., "Expression": ..., "Disabled": ... } ]</c>.
/// </summary>
public sealed class CronJobsOptions : List<CronJobOverride>
{
    public const string SectionName = "CronJobs";
}
