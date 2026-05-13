using AnimeFeedManager.Infrastructure.Cosmos.Types;

namespace AnimeFeedManager.Infrastructure.Cosmos.Outbox;

/// <summary>
/// Marker base for outbox documents. Concrete types declare per-container polymorphic
/// hierarchies via <see cref="System.Text.Json.Serialization.JsonPolymorphicAttribute"/>
/// using <see cref="DiscriminatorPropertyName"/> as the JSON discriminator field name.
/// </summary>
public abstract record OutboxDocument : CosmosDocument
{
    /// <summary>
    /// JSON discriminator field name shared across every per-container outbox hierarchy.
    /// Persisted to Cosmos — renaming is a breaking change.
    /// </summary>
    public const string DiscriminatorPropertyName = "outboxType";

    /// <summary>
    /// UTC timestamp captured when the writer constructs the outbox.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
