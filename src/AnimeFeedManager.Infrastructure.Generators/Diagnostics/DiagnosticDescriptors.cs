using Microsoft.CodeAnalysis;

namespace AnimeFeedManager.Infrastructure.Generators.Diagnostics;

/// <summary>
/// Diagnostic descriptors for Cosmos registry generator.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "CosmosRegistry";

    /// <summary>
    /// COSMOSREG001: Container has conflicting partition keys from different entities.
    /// </summary>
    public static readonly DiagnosticDescriptor ConflictingPartitionKeys = new(
        id: "COSMOSREG001",
        title: "Conflicting partition keys",
        messageFormat: "Container '{0}' has conflicting partition keys: expected '{1}', found '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All entities mapped to the same container must use the same partition key path.");

    /// <summary>
    /// COSMOSREG002: Partition key path must start with '/'.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidPartitionKeyPath = new(
        id: "COSMOSREG002",
        title: "Invalid partition key path",
        messageFormat: "Partition key path '{0}' must start with '/'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Cosmos DB partition key paths must begin with a forward slash.");
}
