using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace AnimeFeedManager.SourceGenerators.TableNames;

[Generator]
public class TableNameMapGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get all classes with WithTableNameAttribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with the compilation
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate the source code
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        // Only process class declarations with attributes
        if (node is not ClassDeclarationSyntax classDeclaration || classDeclaration.AttributeLists.Count == 0)
            return false;
        
        // Filter out non-regular classes
        if (classDeclaration.Modifiers.Any(m => 
                m.IsKind(SyntaxKind.StaticKeyword) || 
                m.IsKind(SyntaxKind.AbstractKeyword) ||
                m.IsKind(SyntaxKind.SealedKeyword)))
            return false;
        
        return true;
    }


    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
    
        // Get the semantic model and symbol for more accurate checks
        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
    
        // Skip if we can't get the symbol or if it's not a regular class
        if (classSymbol == null || classSymbol.IsStatic || classSymbol.IsAbstract || classSymbol.TypeKind != TypeKind.Class)
            return null;
    
        // Check for the WithTableNameAttribute
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                if (attributeName.Contains("WithTableName"))
                {
                    // Now we can be sure it's a regular class with the right attribute
                    return classDeclaration;
                }
            }
        }

        return null;
    }


    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctClasses = classes.Distinct();
        var tableEntities = GetTableEntities(compilation, distinctClasses, context);

        if (tableEntities.Count > 0)
        {
            var source = GenerateTableNameMapSource(tableEntities);
            context.AddSource("TableNameMap.g.cs", source);
        }
    }

    private static List<(string EntityTypeName, string EntityNamespace, string TableName)> GetTableEntities(
        Compilation compilation,
        IEnumerable<ClassDeclarationSyntax?> classes,
        SourceProductionContext context)
    {
        var result = new List<(string, string, string)>();

        var attributeSymbol =
            compilation.GetTypeByMetadataName(
                "AnimeFeedManager.SourceGenerators.TableNames.WithTableNameAttribute");
        if (attributeSymbol == null)
        {
            return result;
        }

        var tableEntitySymbol =
            compilation.GetTypeByMetadataName("Azure.Data.Tables.ITableEntity");
        if (tableEntitySymbol == null)
        {
            // If ITableEntity symbol can't be found, we can't validate implementation
            return result;
        }

        foreach (var classDeclaration in classes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (classDeclaration == null) continue;

            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null)
                continue;

            foreach (var attribute in classSymbol.GetAttributes())
            {
                if (!attribute.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default))
                    continue;

                // Check if the class implements ITableEntity
                if (!ImplementsInterface(classSymbol, tableEntitySymbol))
                {
                    // Report diagnostic error
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            id: "TNM001",
                            title: "Invalid attribute usage",
                            messageFormat:
                            "WithTableNameAttribute can only be applied to classes that implement ITableEntity",
                            category: "TableNameMapGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        classDeclaration.GetLocation()));
                    continue;
                }

                if (attribute.ConstructorArguments.Length == 0)
                    continue;

                var tableNameArg = attribute.ConstructorArguments[0];
                if (tableNameArg.Value is not string tableName)
                    continue;

                result.Add((
                    classSymbol.Name,
                    classSymbol.ContainingNamespace.ToDisplayString(),
                    tableName
                ));

                break;
            }
        }

        return result;
    }

    private static bool ImplementsInterface(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceType)
    {
        return classSymbol.AllInterfaces.Any(i =>
            SymbolEqualityComparer.Default.Equals(i, interfaceType));
    }

    private static string GenerateTableNameMapSource(
        List<(string EntityTypeName, string EntityNamespace, string TableName)> tableEntities)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");

        // Add all the namespaces needed for the entity types
        var namespaces = tableEntities.Select(te => te.EntityNamespace).Distinct().ToList();
        foreach (var ns in namespaces)
        {
            sb.AppendLine($"using {ns};");
        }

        sb.AppendLine();
        sb.AppendLine("namespace AnimeFeedManager.Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Auto-generated map of entity types to their Azure table names.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class AzureTableName");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly Dictionary<Type, string> TypeToTableNameMap = new()");
        sb.AppendLine("    {");

        foreach (var (entityTypeName, entityNamespace, tableName) in tableEntities)
        {
            sb.AppendLine($"        [typeof({entityNamespace}.{entityTypeName})] = \"{tableName}\",");
        }

        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets the table name for the specified entity type.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"entityType\">The type of the entity</param>");
        sb.AppendLine("    /// <returns>The table name for the specified entity type</returns>");
        sb.AppendLine(
            "    /// <exception cref=\"KeyNotFoundException\">Thrown when the entity type is not registered in the table name map</exception>");
        sb.AppendLine("    public static string GetTableName(Type entityType)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (TypeToTableNameMap.TryGetValue(entityType, out var tableName))");
        sb.AppendLine("        {");
        sb.AppendLine("            return tableName;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine(
            "        throw new KeyNotFoundException($\"No table name found for entity type: {entityType.FullName}. Make sure the type has a WithTableNameAttribute.\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets the table name for the specified entity type.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <typeparam name=\"T\">The type of the entity</typeparam>");
        sb.AppendLine("    /// <returns>The table name for the specified entity type</returns>");
        sb.AppendLine(
            "    /// <exception cref=\"KeyNotFoundException\">Thrown when the entity type is not registered in the table name map</exception>");
        sb.AppendLine("    public static string GetTableName<T>()");
        sb.AppendLine("    {");
        sb.AppendLine("        return GetTableName(typeof(T));");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}