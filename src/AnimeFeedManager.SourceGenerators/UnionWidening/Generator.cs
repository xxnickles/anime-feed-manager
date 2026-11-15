using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AnimeFeedManager.SourceGenerators.UnionWidening;

[Generator]
public class UnionWideningGenerator : IIncrementalGenerator
{
    private const string WidensFrom = "global::AnimeFeedManager.Shared.Results.IWidensFrom";
    private const string AttributeNamespace = "AnimeFeedManager.Shared.Results";
    private const string AttributeName = "WidensAttribute";
    private const string IUnion = "System.Runtime.CompilerServices.IUnion";

    private static readonly DiagnosticDescriptor NotAUnion = Error("WID001", "Consumed type is not a union",
        "'{0}' cannot widen from '{1}': it is not a union");

    private static readonly DiagnosticDescriptor NotASubset = Error("WID002", "Consumed union is not a subset",
        "'{0}' cannot widen from '{1}': case '{2}' is not a case of '{0}'");

    private static readonly DiagnosticDescriptor AlreadyACase = Error("WID003", "Consumed type is already a case",
        "'{0}' already widens from its case '{1}'; remove it from [Widens]");

    private static readonly DiagnosticDescriptor Unsupported = Error("WID004", "Unsupported union shape",
        "'{0}' must be a non-generic, top-level union for [Widens]");

    private static readonly DiagnosticDescriptor NotPartial = Error("WID005", "Union is not partial",
        "'{0}' must be declared partial for [Widens]");

    private static readonly DiagnosticDescriptor DuplicateAttribute = Error("WID006", "Multiple [Widens] attributes",
        "'{0}' takes a single [Widens] attribute; list every consumed union in it");

    private static readonly DiagnosticDescriptor ListedTwice = Error("WID007", "Consumed union listed twice",
        "'{0}' lists '{1}' more than once in [Widens]");

    private static readonly DiagnosticDescriptor TargetNotAUnion = Error("WID008", "[Widens] target is not a union",
        "'{0}' is not a union; [Widens] applies to union declarations only");

    private static readonly ImmutableDictionary<string, DiagnosticDescriptor> Descriptors =
        new[] { NotAUnion, NotASubset, AlreadyACase, Unsupported, NotPartial, DuplicateAttribute, ListedTwice, TargetNotAUnion }
            .ToImmutableDictionary(d => d.Id);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var unions = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is TypeDeclarationSyntax { AttributeLists.Count: > 0 }
                                               and (UnionDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax),
                transform: static (ctx, ct) => Build(ctx, ct))
            .Where(static output => output is not null)
            .Select(static (output, _) => output!);

        context.RegisterSourceOutput(unions, static (spc, output) => Emit(spc, output));
    }

    private static WideningOutput? Build(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        var declaration = (TypeDeclarationSyntax)ctx.Node;
        if (ctx.SemanticModel.GetDeclaredSymbol(declaration, ct) is not INamedTypeSymbol self)
            return null;

        var widens = self.GetAttributes().Where(IsWidens).ToList();
        if (widens.Count == 0 || !DeclaredOn(widens[0], declaration))
            return null;

        var hintName = $"{self.ToDisplayString()}.Widen.g.cs";
        var unionLocation = declaration.Identifier.GetLocation();

        if (declaration is not UnionDeclarationSyntax)
            return WideningOutput.Failed(hintName, Diag(TargetNotAUnion, unionLocation, self.Name));
        if (widens.Count > 1)
            return WideningOutput.Failed(hintName, Diag(DuplicateAttribute, unionLocation, self.Name));
        if (!declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            return WideningOutput.Failed(hintName, Diag(NotPartial, unionLocation, self.Name));
        if (self.ContainingType is not null || self.IsGenericType)
            return WideningOutput.Failed(hintName, Diag(Unsupported, unionLocation, self.Name));

        var attributeLocation = widens[0].ApplicationSyntaxReference?.GetSyntax(ct).GetLocation();
        var cases = CasesOf(self);
        var consumed = new List<(string Union, ImmutableArray<string> Cases)>();
        var diagnostics = new List<DiagnosticInfo>();
        var listed = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var violatesConstraint = false;

        foreach (var source in widens[0].AttributeClass!.TypeArguments)
        {
            if (!listed.Add(source))
            {
                diagnostics.Add(Diag(ListedTwice, attributeLocation, self.Name, source.Name));
                continue;
            }

            // The attribute's `struct, IUnion` constraint already reports these (CS0453 / CS0315).
            if (!source.IsValueType || !source.AllInterfaces.Any(i => i.ToDisplayString() == IUnion))
            {
                violatesConstraint = true;
                continue;
            }

            if (cases.Contains(source, SymbolEqualityComparer.Default))
            {
                diagnostics.Add(Diag(AlreadyACase, attributeLocation, self.Name, source.Name));
                continue;
            }

            if (source is not INamedTypeSymbol { IsUnion: true } sourceUnion)
            {
                diagnostics.Add(Diag(NotAUnion, attributeLocation, self.Name, source.Name));
                continue;
            }

            var sourceCases = CasesOf(sourceUnion);
            var missing = sourceCases.FirstOrDefault(c => !cases.Contains(c, SymbolEqualityComparer.Default));
            if (missing is not null)
            {
                diagnostics.Add(Diag(NotASubset, attributeLocation, self.Name, source.Name, missing.Name));
                continue;
            }

            consumed.Add((Qualified(source), sourceCases.Select(Qualified).ToImmutableArray()));
        }

        return diagnostics.Count > 0 || violatesConstraint
            ? WideningOutput.Failed(hintName, diagnostics.ToArray())
            : new WideningOutput(hintName, Render(self, cases.Select(Qualified).ToList(), consumed),
                ImmutableArray<DiagnosticInfo>.Empty);
    }

    private static bool IsWidens(AttributeData attribute) =>
        attribute.AttributeClass is { Name: AttributeName } attributeClass
        && attributeClass.ContainingNamespace.ToDisplayString() == AttributeNamespace;

    // A partial union is visited once per declaration; only the one carrying [Widens] emits.
    private static bool DeclaredOn(AttributeData attribute, SyntaxNode declaration) =>
        attribute.ApplicationSyntaxReference is { } reference
        && reference.SyntaxTree == declaration.SyntaxTree
        && declaration.Span.Contains(reference.Span);

    // A union's compiler-synthesized single-parameter constructors are exactly its cases; user-declared ones are an error.
    private static ImmutableArray<ITypeSymbol> CasesOf(INamedTypeSymbol union) =>
        union.InstanceConstructors
            .Where(ctor => ctor.Parameters.Length == 1)
            .Select(ctor => ctor.Parameters[0].Type)
            .ToImmutableArray();

    private static string Qualified(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    private static string Render(INamedTypeSymbol self, List<string> cases,
        List<(string Union, ImmutableArray<string> Cases)> consumed)
    {
        var name = self.Name;
        var families = cases.Concat(consumed.Select(c => c.Union)).ToList();
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        if (!self.ContainingNamespace.IsGlobalNamespace)
        {
            sb.AppendLine($"namespace {self.ContainingNamespace.ToDisplayString()};");
            sb.AppendLine();
        }

        sb.AppendLine($"partial union {name}");
        for (var i = 0; i < families.Count; i++)
            sb.AppendLine($"    {(i == 0 ? ":" : ",")} {WidensFrom}<{name}, {families[i]}>");
        sb.AppendLine("{");

        // The compiler converts a case into its union; a consumed union needs a user-defined conversion.
        foreach (var (union, unionCases) in consumed)
        {
            sb.AppendLine($"    public static implicit operator {name}({union} error) => error switch");
            sb.AppendLine("    {");
            for (var i = 0; i < unionCases.Length; i++)
                sb.AppendLine($"        {unionCases[i]} case{i} => case{i}{(i < unionCases.Length - 1 ? "," : "")}");
            sb.AppendLine("    };");
            sb.AppendLine();
        }

        foreach (var family in families)
            sb.AppendLine($"    static {name} {WidensFrom}<{name}, {family}>.Widen({family} error) => error;");

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void Emit(SourceProductionContext spc, WideningOutput output)
    {
        foreach (var info in output.Diagnostics)
            spc.ReportDiagnostic(Diagnostic.Create(Descriptors[info.Id],
                info.FilePath.Length == 0 ? null : Location.Create(info.FilePath, info.Span, info.LineSpan),
                info.Arg0, info.Arg1, info.Arg2));

        if (output.Source is not null)
            spc.AddSource(output.HintName, SourceText.From(output.Source, Encoding.UTF8));
    }

    private static DiagnosticDescriptor Error(string id, string title, string message) =>
        new(id, title, message, "UnionWideningGenerator", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static DiagnosticInfo Diag(DiagnosticDescriptor descriptor, Location? location, string arg0,
        string arg1 = "", string arg2 = "") =>
        new(descriptor.Id, location?.SourceTree?.FilePath ?? "", location?.SourceSpan ?? default,
            location?.GetLineSpan().Span ?? default, arg0, arg1, arg2);

    // Location roots the syntax tree, so diagnostics carry its equatable parts across the incremental cache.
    private record struct DiagnosticInfo(string Id, string FilePath, TextSpan Span, LinePositionSpan LineSpan,
        string Arg0, string Arg1, string Arg2);

    private sealed class WideningOutput(string hintName, string? source, ImmutableArray<DiagnosticInfo> diagnostics)
        : IEquatable<WideningOutput>
    {
        public string HintName { get; } = hintName;
        public string? Source { get; } = source;
        public ImmutableArray<DiagnosticInfo> Diagnostics { get; } = diagnostics;

        public static WideningOutput Failed(string hintName, params DiagnosticInfo[] diagnostics) =>
            new(hintName, null, diagnostics.ToImmutableArray());

        public bool Equals(WideningOutput? other) =>
            other is not null && HintName == other.HintName && Source == other.Source
            && Diagnostics.SequenceEqual(other.Diagnostics);

        public override bool Equals(object? obj) => Equals(obj as WideningOutput);
        public override int GetHashCode() => (HintName, Source).GetHashCode();
    }
}
