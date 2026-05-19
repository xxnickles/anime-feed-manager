using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnimeFeedManager.Infrastructure.Generators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AnimeFeedManager.Infrastructure.Generators.CosmosRegistry;

/// <summary>
/// Incremental source generator that:
/// 1. Scans all referenced assemblies for [CosmosEntity] decorated types
/// 2. Expands [JsonDerivedType] on each [CosmosEntity]-decorated type, registering every
///    listed derived type against the base's container/PK (unless the derived type carries
///    its own [CosmosEntity], which overrides the inherited mapping)
/// 3. Generates CosmosContainerRegistry with the aggregated mappings
///
/// Usage: Reference this generator from your composition root (e.g., Functions project).
/// The generated registry should be passed to AddCosmosInfrastructure().
/// </summary>
[Generator]
public class CosmosRegistryGenerator : IIncrementalGenerator
{
    private const string AttributeShortName = "CosmosEntityAttribute";
    private const string JsonDerivedTypeAttributeShortName = "JsonDerivedTypeAttribute";
    private const string JsonDerivedTypeAttributeNamespace = "System.Text.Json.Serialization";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationProvider = context.CompilationProvider;
        context.RegisterSourceOutput(compilationProvider, Execute);
    }

    private static void Execute(SourceProductionContext context, Compilation compilation)
    {
        // Discover the attribute type and all decorated entities
        var (entities, attributeNamespace) = DiscoverEntities(compilation);

        // Validate and generate registry
        if (entities.Count == 0)
        {
            EmitEmptyRegistry(context, compilation.AssemblyName, attributeNamespace);
            return;
        }

        var containerPartitionKeys = ValidateEntities(context, entities);
        EmitRegistry(context, compilation.AssemblyName, attributeNamespace, entities, containerPartitionKeys);
    }

    private static (List<EntityInfo> Entities, string AttributeNamespace) DiscoverEntities(Compilation compilation)
    {
        var entities = new List<EntityInfo>();
        var seenTypes = new HashSet<string>();

        var attributeSymbol = FindAttributeSymbol(compilation);
        if (attributeSymbol == null)
            return (entities, string.Empty);

        // Scan all referenced assemblies for types with [CosmosEntity]
        foreach (var reference in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assemblySymbol == null)
                continue;

            DiscoverEntitiesInNamespace(assemblySymbol.GlobalNamespace, attributeSymbol, entities, seenTypes);
        }

        // Also scan the current compilation (may find types defined in the current project)
        DiscoverEntitiesInNamespace(compilation.GlobalNamespace, attributeSymbol, entities, seenTypes);

        return (entities, attributeSymbol.ContainingNamespace.ToDisplayString());
    }

    /// <summary>
    /// Finds CosmosEntityAttribute using O(1) metadata lookup first,
    /// falling back to a targeted scan of Infrastructure assemblies only.
    /// </summary>
    private static INamedTypeSymbol? FindAttributeSymbol(Compilation compilation)
    {
        // Fast path: O(1) hash lookup by fully qualified metadata name
        var symbol = compilation.GetTypeByMetadataName(
            $"{compilation.AssemblyName?.Split('.')[0]}.Infrastructure.Cosmos.{AttributeShortName}");
        if (symbol != null)
            return symbol;

        // Fallback: scan only assemblies whose name contains "Infrastructure"
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
                continue;

            if (!assembly.Name.Contains("Infrastructure"))
                continue;

            var candidate = FindTypeInNamespace(assembly.GlobalNamespace, AttributeShortName);
            if (candidate != null)
                return candidate;
        }

        return null;
    }

    private static INamedTypeSymbol? FindTypeInNamespace(INamespaceSymbol ns, string typeName)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol childNs)
            {
                var found = FindTypeInNamespace(childNs, typeName);
                if (found != null) return found;
            }
            else if (member is INamedTypeSymbol type && type.Name == typeName)
            {
                return type;
            }
        }

        return null;
    }

    private static void DiscoverEntitiesInNamespace(
        INamespaceSymbol namespaceSymbol,
        INamedTypeSymbol attributeSymbol,
        List<EntityInfo> entities,
        HashSet<string> seenTypes)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamespaceSymbol childNamespace)
            {
                DiscoverEntitiesInNamespace(childNamespace, attributeSymbol, entities, seenTypes);
            }
            else if (member is INamedTypeSymbol typeSymbol)
            {
                TryAddEntityFromType(typeSymbol, attributeSymbol, entities, seenTypes);
            }
        }
    }

    private static void TryAddEntityFromType(
        INamedTypeSymbol typeSymbol,
        INamedTypeSymbol attributeSymbol,
        List<EntityInfo> entities,
        HashSet<string> seenTypes)
    {
        if (typeSymbol.TypeKind != TypeKind.Class)
            return;

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol))
                continue;

            var args = attribute.ConstructorArguments;
            if (args.Length < 2)
                continue;

            var containerName = args[0].Value as string;
            var partitionKeyPath = args[1].Value as string;

            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(partitionKeyPath))
                continue;

            var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            // Skip if we've already registered this type (cross-source dedup, or already
            // emitted as a derived expansion from another base's [JsonDerivedType])
            if (!seenTypes.Add(fullTypeName))
                return;

            entities.Add(new EntityInfo(
                typeName: typeSymbol.Name,
                fullTypeName: fullTypeName,
                containerName: containerName!,
                partitionKeyPath: partitionKeyPath!));

            AddDerivedEntries(typeSymbol, attributeSymbol, containerName!, partitionKeyPath!, entities, seenTypes);
        }
    }

    /// <summary>
    /// Expands [JsonDerivedType] attributes on a [CosmosEntity]-decorated base type and emits
    /// a registry entry for each listed derived type under the base's container/PK.
    /// Skips derived types that carry their own [CosmosEntity] — their declared mapping wins.
    /// </summary>
    private static void AddDerivedEntries(
        INamedTypeSymbol baseTypeSymbol,
        INamedTypeSymbol cosmosEntityAttributeSymbol,
        string containerName,
        string partitionKeyPath,
        List<EntityInfo> entities,
        HashSet<string> seenTypes)
    {
        foreach (var attribute in baseTypeSymbol.GetAttributes())
        {
            var attrClass = attribute.AttributeClass;
            if (attrClass is null)
                continue;
            if (attrClass.Name != JsonDerivedTypeAttributeShortName)
                continue;
            if (attrClass.ContainingNamespace?.ToDisplayString() != JsonDerivedTypeAttributeNamespace)
                continue;

            var args = attribute.ConstructorArguments;
            if (args.Length < 1)
                continue;

            if (args[0].Value is not INamedTypeSymbol derivedType)
                continue;

            // Override semantics: a derived type with its own [CosmosEntity] declares its own
            // mapping; don't double-register here. The normal scan will pick up its attribute.
            if (HasCosmosEntityAttribute(derivedType, cosmosEntityAttributeSymbol))
                continue;

            var derivedFullName = derivedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (!seenTypes.Add(derivedFullName))
                continue;

            entities.Add(new EntityInfo(
                typeName: derivedType.Name,
                fullTypeName: derivedFullName,
                containerName: containerName,
                partitionKeyPath: partitionKeyPath));
        }
    }

    private static bool HasCosmosEntityAttribute(INamedTypeSymbol typeSymbol, INamedTypeSymbol attributeSymbol)
    {
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol))
                return true;
        }
        return false;
    }

    private static Dictionary<string, (string PartitionKey, EntityInfo FirstEntity)> ValidateEntities(
        SourceProductionContext context,
        List<EntityInfo> entities)
    {
        var containerPartitionKeys = new Dictionary<string, (string PartitionKey, EntityInfo FirstEntity)>();

        foreach (var entity in entities)
        {
            if (!entity.PartitionKeyPath.StartsWith("/"))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidPartitionKeyPath,
                    Location.None,
                    entity.PartitionKeyPath));
                continue;
            }

            if (containerPartitionKeys.TryGetValue(entity.ContainerName, out var existing))
            {
                if (existing.PartitionKey != entity.PartitionKeyPath)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.ConflictingPartitionKeys,
                        Location.None,
                        entity.ContainerName,
                        existing.PartitionKey,
                        entity.PartitionKeyPath));
                    continue;
                }
            }
            else
            {
                containerPartitionKeys[entity.ContainerName] = (entity.PartitionKeyPath, entity);
            }
        }

        return containerPartitionKeys;
    }

    private static void EmitEmptyRegistry(SourceProductionContext context, string? assemblyName, string attributeNamespace = "")
    {
        var ns = assemblyName ?? "Generated";
        var usingLine = string.IsNullOrEmpty(attributeNamespace) ? "" : $"using {attributeNamespace};\n";
        var source = $@"// <auto-generated/>
#nullable enable

using System;
using System.Collections.Generic;
{usingLine}

namespace {ns};

/// <summary>
/// Auto-generated registry of Cosmos DB containers and their partition keys.
/// No entities with [CosmosEntity] attribute were found.
/// </summary>
public static class CosmosContainerRegistry
{{
    public static IReadOnlyDictionary<Type, ContainerInfo> EntityRegistry {{ get; }} =
        new Dictionary<Type, ContainerInfo>();

    public static IReadOnlyDictionary<string, string> ContainerPartitionKeys {{ get; }} =
        new Dictionary<string, string>();

    public static ContainerInfo? GetContainerInfo<T>() => null;

    public static ContainerInfo? GetContainerInfo(Type entityType) => null;
}}
";
        context.AddSource("CosmosContainerRegistry.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static void EmitRegistry(
        SourceProductionContext context,
        string? assemblyName,
        string attributeNamespace,
        List<EntityInfo> entities,
        Dictionary<string, (string PartitionKey, EntityInfo FirstEntity)> containerPartitionKeys)
    {
        var ns = assemblyName ?? "Generated";
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine($"using {attributeNamespace};");
        sb.AppendLine();
        sb.AppendLine($"namespace {ns};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Auto-generated registry of Cosmos DB containers and their partition keys.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class CosmosContainerRegistry");
        sb.AppendLine("{");

        // EntityRegistry dictionary
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Maps entity types to their container info.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IReadOnlyDictionary<Type, ContainerInfo> EntityRegistry { get; } =");
        sb.AppendLine("        new Dictionary<Type, ContainerInfo>");
        sb.AppendLine("        {");
        foreach (var entity in entities.OrderBy(e => e.FullTypeName))
        {
            sb.AppendLine($"            {{ typeof({entity.FullTypeName}), new ContainerInfo(\"{entity.ContainerName}\", \"{entity.PartitionKeyPath}\") }},");
        }
        sb.AppendLine("        };");
        sb.AppendLine();

        // ContainerPartitionKeys - unique containers for AppHost container creation
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Unique container names with their partition key paths.");
        sb.AppendLine("    /// Use this in AppHost to create Cosmos containers.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IReadOnlyDictionary<string, string> ContainerPartitionKeys { get; } =");
        sb.AppendLine("        new Dictionary<string, string>");
        sb.AppendLine("        {");
        foreach (var kvp in containerPartitionKeys.OrderBy(k => k.Key))
        {
            sb.AppendLine($"            {{ \"{kvp.Key}\", \"{kvp.Value.PartitionKey}\" }},");
        }
        sb.AppendLine("        };");
        sb.AppendLine();

        // GetContainerInfo<T>()
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets the container info for the specified entity type.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static ContainerInfo? GetContainerInfo<T>() =>");
        sb.AppendLine("        EntityRegistry.TryGetValue(typeof(T), out var info) ? info : null;");
        sb.AppendLine();

        // GetContainerInfo(Type)
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets the container info for the specified entity type.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static ContainerInfo? GetContainerInfo(Type entityType) =>");
        sb.AppendLine("        EntityRegistry.TryGetValue(entityType, out var info) ? info : null;");

        sb.AppendLine("}");

        context.AddSource("CosmosContainerRegistry.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private readonly struct EntityInfo
    {
        public string TypeName { get; }
        public string FullTypeName { get; }
        public string ContainerName { get; }
        public string PartitionKeyPath { get; }

        public EntityInfo(string typeName, string fullTypeName, string containerName, string partitionKeyPath)
        {
            TypeName = typeName;
            FullTypeName = fullTypeName;
            ContainerName = containerName;
            PartitionKeyPath = partitionKeyPath;
        }
    }
}
