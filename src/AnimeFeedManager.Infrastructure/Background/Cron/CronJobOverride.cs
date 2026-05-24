namespace AnimeFeedManager.Infrastructure.Background.Cron;

/// <summary>
/// Single entry in the <c>CronJobs</c> configuration array. Matched to a registered
/// <see cref="CronJobRegistration"/> by <see cref="Name"/>. Either field may be omitted;
/// an entry that sets neither <see cref="Expression"/> nor <see cref="Disabled"/> has no effect.
/// </summary>
public class CronJobOverride
{
    /// <summary>Registered job name (case-sensitive). Required.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Overrides the code-side default expression. Null or empty keeps the default.
    /// A malformed expression is logged at startup/reload and the default is used.
    /// </summary>
    public string? Expression { get; set; }

    /// <summary>When true, the job is skipped entirely until configuration changes.</summary>
    public bool Disabled { get; set; }
}
