using System.Text.Json.Serialization;

namespace AnimeFeedManager.Infrastructure.Cosmos.Types;

/// <summary>
/// Base record for all Cosmos DB documents. Owns the <c>id</c> property
/// so that every entity inherits a consistent identity contract.
/// </summary>
public abstract record CosmosDocument
{
    /// <summary>
    /// Defaults to a fresh UUID v7. Override on derived types whose ID is derived
    /// from business keys or requires a customized format (e.g. <c>DuplicateDocument.CreateId</c>).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.CreateVersion7().ToString();
}
