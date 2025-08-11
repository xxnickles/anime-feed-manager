using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AnimeFeedManager.SourceGenerators.QueueNames;

[Generator]
public class QueueNamesSourceGenerator : IIncrementalGenerator
{
    private class DomainMessageInfo(
        string typeName,
        string fullTypeName, 
        string queueConstantName,
        string queueConstantValue,
        string @namespace
    )
    {
        public string TypeName { get; } = typeName;
        public string FullTypeName { get; } = fullTypeName;
        public string QueueConstantName { get; } = queueConstantName;
        public string QueueConstantValue { get; } = queueConstantValue;
        public string Namespace { get; } = @namespace;
      
        
    }

    private class QueueConstantInfo(string name, string value)
    {
        public string Name { get; } = name;
        public string Value { get; } = value;
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Create a pipeline to find all classes that inherit from DomainMessage
        var domainMessageProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsDomainMessageCandidate(s),
                transform: static (ctx, _) => GetDomainMessageInfo(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Collect all queue names and generate the aggregated class
        context.RegisterSourceOutput(domainMessageProvider.Collect(), static (spc, source) => Execute(source, spc));
    }

    private static bool IsDomainMessageCandidate(SyntaxNode node)
    {
        // Look for record or class declarations that might inherit from DomainMessage
        return node is RecordDeclarationSyntax record && record.BaseList is not null ||
               node is ClassDeclarationSyntax @class && @class.BaseList is not null;
    }

    private static DomainMessageInfo? GetDomainMessageInfo(GeneratorSyntaxContext context)
    {
        var node = context.Node;
        SemanticModel semanticModel = context.SemanticModel;

        // Handle both records and classes
        BaseTypeDeclarationSyntax? typeDeclaration = node switch
        {
            RecordDeclarationSyntax record => record,
            ClassDeclarationSyntax @class => @class,
            _ => null
        };

        if (typeDeclaration?.BaseList is null)
            return null;

        // Get the symbol for this type
        if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol)
            return null;

        // Check if it inherits from DomainMessage
        if (!InheritsFromDomainMessage(typeSymbol))
            return null;

        // Look for const string fields with "queue" in the name (case-insensitive)
        var queueConstant = FindQueueConstant(typeDeclaration);
        
        if (queueConstant is null)
            return null;

        return new DomainMessageInfo(
            typeName: typeSymbol.Name,
            fullTypeName: typeSymbol.ToDisplayString(),
            queueConstantName: queueConstant.Name,
            queueConstantValue: queueConstant.Value,
            @namespace: typeSymbol.ContainingNamespace.ToDisplayString()
        );
    }

    private static bool InheritsFromDomainMessage(INamedTypeSymbol typeSymbol)
    {
        var currentType = typeSymbol.BaseType;
        while (currentType is not null)
        {
            if (currentType.Name == "DomainMessage")
                return true;
            currentType = currentType.BaseType;
        }
        return false;
    }

    private static QueueConstantInfo? FindQueueConstant(BaseTypeDeclarationSyntax typeDeclaration)
    {
        // Get members based on the specific type
        SyntaxList<MemberDeclarationSyntax> members = typeDeclaration switch
        {
            RecordDeclarationSyntax record => record.Members,
            ClassDeclarationSyntax @class => @class.Members,
            _ => default
        };

        // Look for const string fields with "queue" in the name (case-insensitive)
        var constantFields = members
            .OfType<FieldDeclarationSyntax>()
            .Where(f => f.Modifiers.Any(SyntaxKind.ConstKeyword))
            .Where(f => f.Declaration.Type is PredefinedTypeSyntax pts && 
                        pts.Keyword.IsKind(SyntaxKind.StringKeyword));

        foreach (var field in constantFields)
        {
            foreach (var variable in field.Declaration.Variables)
            {
                var fieldName = variable.Identifier.ValueText;
            
                // Check if field name contains "queue" (case-insensitive)
                if (fieldName.IndexOf("queue", StringComparison.OrdinalIgnoreCase) < 0 &&
                    !fieldName.Contains("Queue")) continue;
                
                // Extract the string literal value
                if (variable.Initializer?.Value is not LiteralExpressionSyntax literal ||
                    !literal.Token.IsKind(SyntaxKind.StringLiteralToken)) continue;
                
                var value = literal.Token.ValueText;
                return new QueueConstantInfo(fieldName, value);
            }
        }

        return null;
    }



    private static void Execute(ImmutableArray<DomainMessageInfo> domainMessages, SourceProductionContext context)
    {
        if (domainMessages.IsEmpty)
            return;

        var sourceBuilder = new StringBuilder();
        
        sourceBuilder.AppendLine("// <auto-generated />");
        sourceBuilder.AppendLine("using System;");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("namespace AnimeFeedManager.Generated;");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("/// <summary>");
        sourceBuilder.AppendLine("/// Contains all queue names discovered from DomainMessage implementations.");
        sourceBuilder.AppendLine("/// This class is automatically generated.");
        sourceBuilder.AppendLine("/// </summary>");
        sourceBuilder.AppendLine("public static class DomainMessageQueues");
        sourceBuilder.AppendLine("{");

        // Generate individual constants
        foreach (var message in domainMessages.OrderBy(m => m.TypeName))
        {
            sourceBuilder.AppendLine($"    /// <summary>");
            sourceBuilder.AppendLine($"    /// Queue name for {message.FullTypeName}");
            sourceBuilder.AppendLine($"    /// </summary>");
            sourceBuilder.AppendLine($"    public const string {message.TypeName} = \"{message.QueueConstantValue}\";");
            sourceBuilder.AppendLine();
        }

        // Generate array with all queue names
        sourceBuilder.AppendLine("    /// <summary>");
        sourceBuilder.AppendLine("    /// Array containing all discovered queue names.");
        sourceBuilder.AppendLine("    /// </summary>");
        sourceBuilder.Append("    public static readonly string[] AllQueues = {");
        
        if (domainMessages.Length > 0)
        {
            sourceBuilder.AppendLine();
            for (int i = 0; i < domainMessages.Length; i++)
            {
                var message = domainMessages[i];
                sourceBuilder.Append($"        {message.TypeName}");
                if (i < domainMessages.Length - 1)
                    sourceBuilder.Append(",");
                sourceBuilder.AppendLine();
            }
            sourceBuilder.AppendLine("    };");
        }
        else
        {
            sourceBuilder.AppendLine(" };");
        }

        sourceBuilder.AppendLine();

        // Generate ReadOnlySpan property for performance
        sourceBuilder.AppendLine("    /// <summary>");
        sourceBuilder.AppendLine("    /// ReadOnlySpan containing all discovered queue names for efficient iteration.");
        sourceBuilder.AppendLine("    /// </summary>");
        sourceBuilder.AppendLine("    public static ReadOnlySpan<string> AllQueuesSpan => AllQueues;");

        sourceBuilder.AppendLine();

        // Generate count property
        sourceBuilder.AppendLine("    /// <summary>");
        sourceBuilder.AppendLine("    /// Total number of discovered queues.");
        sourceBuilder.AppendLine("    /// </summary>");
        sourceBuilder.AppendLine($"    public const int QueueCount = {domainMessages.Length};");

        sourceBuilder.AppendLine("}");

        context.AddSource("DomainMessageQueues.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }


}