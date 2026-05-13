; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
COSMOSREG001 | CosmosRegistry | Error | Conflicting partition keys for same container
COSMOSREG002 | CosmosRegistry | Error | Invalid partition key path (must start with '/')
