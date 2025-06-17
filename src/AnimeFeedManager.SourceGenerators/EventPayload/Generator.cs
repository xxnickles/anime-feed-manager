
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace AnimeFeedManager.SourceGenerators.EventPayload;

[Generator]
public class EventPayloadSerializerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get all classes with EventPayloadSerializerContextAttribute
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

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
        node is ClassDeclarationSyntax {AttributeLists: {Count: > 0}};

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax) context.Node;

        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                if (attributeName.Contains("EventPayloadSerializerContext"))
                {
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
        var payloadContexts = GetPayloadContexts(compilation, distinctClasses, context);

        if (payloadContexts.Count > 0)
        {
            var source = GenerateContextMapSource(payloadContexts);
            context.AddSource("EventPayloadContextMap.g.cs", source);
        }
    }

    private static List<(string ContextTypeName, string ContextNamespace, string PayloadTypeName, string PayloadTypeFullName)> GetPayloadContexts(
        Compilation compilation,
        IEnumerable<ClassDeclarationSyntax?> classes,
        SourceProductionContext context)
    {
        var result = new List<(string, string, string, string)>();

        var attributeSymbol =
            compilation.GetTypeByMetadataName(
                "AnimeFeedManager.SourceGenerators.EventPayload.EventPayloadSerializerContextAttribute");
        if (attributeSymbol == null)
        {
            return result;
        }

        var jsonSerializerContextSymbol =
            compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonSerializerContext");
        if (jsonSerializerContextSymbol == null)
        {
            // If JsonSerializerContext symbol can't be found, we can't validate inheritance
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

                // Check if the class inherits from JsonSerializerContext
                if (!InheritsFrom(classSymbol, jsonSerializerContextSymbol))
                {
                    // Report diagnostic error
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            id: "EPG001",
                            title: "Invalid attribute usage",
                            messageFormat:
                            "EventPayloadSerializerContextAttribute can only be applied to classes that derive from JsonSerializerContext",
                            category: "EventPayloadGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        classDeclaration.GetLocation()));
                    continue;
                }

                if (attribute.ConstructorArguments.Length == 0)
                    continue;

                var payloadTypeArg = attribute.ConstructorArguments[0];
                if (payloadTypeArg.Value is not INamedTypeSymbol payloadType)
                    continue;

                result.Add((
                    classSymbol.Name,
                    classSymbol.ContainingNamespace.ToDisplayString(),
                    payloadType.Name,
                    payloadType.ToDisplayString()
                ));

                break;
            }
        }

        return result;
    }

    private static bool InheritsFrom(INamedTypeSymbol classSymbol, INamedTypeSymbol baseType)
    {
        var currentSymbol = classSymbol.BaseType;

        while (currentSymbol != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentSymbol, baseType))
            {
                return true;
            }

            currentSymbol = currentSymbol.BaseType;
        }

        return false;
    }


    private static string GenerateContextMapSource(
        List<(string ContextTypeName, string ContextNamespace, string PayloadTypeName, string PayloadTypeFullName)> payloadContexts)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Text.Json;");
        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine("using System.Text.Json.Serialization.Metadata;");

        // Add all the namespaces needed for the context types
        var namespaces = payloadContexts.Select(pc => pc.ContextNamespace).Distinct().ToList();
        foreach (var ns in namespaces)
        {
            sb.AppendLine($"using {ns};");
        }

        sb.AppendLine();
        sb.AppendLine("namespace AnimeFeedManager.Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Auto-generated map of event payload types to their JSON serializer contexts.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class EventPayloadContextMap");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly Dictionary<string, (JsonSerializerContext Context, Type PayloadType)> ContextMap = new()");
        sb.AppendLine("    {");

        foreach (var (contextTypeName, _, payloadTypeName, payloadTypeFullName) in payloadContexts)
        {
            sb.AppendLine($"        [\"{payloadTypeName}\"] = ({contextTypeName}.Default, typeof({payloadTypeFullName})),");
        }

        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets the JSON serializer context and payload type for the specified event payload type name.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static (JsonSerializerContext Context, Type PayloadType) GetPayloadContextInfo(string payloadTypeName)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (ContextMap.TryGetValue(payloadTypeName, out var contextInfo))");
        sb.AppendLine("        {");
        sb.AppendLine("            return contextInfo;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine(
            "        throw new KeyNotFoundException($\"No serializer context found for payload type: {payloadTypeName}. Make sure the type has a JsonSerializerContext class marked with EventPayloadSerializerContextAttribute.\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets just the JSON serializer context for the specified event payload type name.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static JsonSerializerContext GetJsonSerializerContext(string payloadTypeName)");
        sb.AppendLine("    {");
        sb.AppendLine("        return GetPayloadContextInfo(payloadTypeName).Context;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}