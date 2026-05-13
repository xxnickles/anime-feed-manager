namespace AnimeFeedManager.Infrastructure.Cosmos;

/// <summary>
/// Configuration options for the Cosmos DB client.
/// </summary>
public class CosmosOptions
{
    public const string SectionName = "Cosmos";

    /// <summary>
    /// Cosmos DB database name.
    /// Must match the database created in the target Cosmos account.
    /// For the emulator, this is created automatically by AppHost.
    /// </summary>
    public string DatabaseName { get; set; } = "duplicates-db";

    /// <summary>
    /// Enable SDK bulk execution for concurrent point operations.
    /// Must be <c>false</c> for the Linux preview emulator (emulator limitations impact bulk updates).
    /// Set to <c>true</c> when targeting a real Cosmos DB instance for production-like throughput.
    /// </summary>
    public bool AllowBulkExecution { get; set; } = true;

    /// <summary>
    /// Target autoscale max throughput (RU/s) to apply to the duplicates container before import.
    /// The container is scaled back to the original value after import completes.
    /// Set to 0 to disable throughput scaling.
    /// </summary>
    public int ImportDuplicatesTargetTmax { get; set; }

    /// <summary>
    /// Target autoscale max throughput (RU/s) to apply to the archive container before import.
    /// The container is scaled back to the original value after import completes.
    /// Set to 0 to disable throughput scaling.
    /// </summary>
    public int ImportArchiveTargetTmax { get; set; }
}
