# AnimeFeedManager.SourceGenerators

**Date:** 2026-05-02

This is a Roslyn `IIncrementalGenerator` project targeting `netstandard2.0`. It is not referenced as a normal library — consuming projects add it as an Analyzer so the generated code participates in compilation without a runtime assembly dependency:

```xml
<ProjectReference Include="..\AnimeFeedManager.SourceGenerators\AnimeFeedManager.SourceGenerators.csproj"
                  OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

Each generator lives in its own folder with a `Generator.cs` entry point. Five generators are currently defined.

---

## Generators

### EventPayload

Synthesizes an internal `EventPayloadSerializerContextAttribute` and an `EventPayloadContextMap` lookup table for classes that inherit from `JsonSerializerContext` and are decorated with that attribute.

| Detail | Value |
|---|---|
| Trigger | Class decorated with `[EventPayloadSerializerContext]` |
| Required base type | `JsonSerializerContext` |
| Output namespace | Same namespace as the annotated class |
| Key emitted symbol | `EventPayloadContextMap` (static lookup) |
| Consumer wiring | None beyond the project reference |

**Diagnostics**

| ID | Condition |
|---|---|
| `EPG001` | Decorated class does not inherit from `JsonSerializerContext` |
| `EPG002` | Payload type does not derive from `SystemNotificationPayload` |

---

### HyperscriptScripts

Compiles colocated `.razor.hs` files into Blazor partial classes, each exposing the file content as a `private const string Script` field. This works around the Razor compiler's `RZ9986` error, which fires when a multi-line `_=` attribute value contains `<...>` markup — a common pattern in hyperscript expressions. Moving the script content to a `.razor.hs` sidecar and referencing it as `_="@Script"` avoids the Razor parser entirely.

| Detail | Value |
|---|---|
| Trigger | `AdditionalText` items whose path ends with `.razor.hs` |
| Input | Plain text file — not Razor markup |
| Output | One `partial class <ClassName>` per `.razor.hs` file |
| Output namespace | Derived from consumer `RootNamespace` + relative directory path |
| Emitted field | `private const string Script = """<file content>""";` |
| Consumer wiring | See below |

**Convention:** the `.razor.hs` filename stem becomes the partial class name. `Foo.razor.hs` next to `Foo.razor` produces `partial class Foo`. This mirrors the `.razor.cs` / `.razor.css` colocated-file pattern.

**No `@@` Razor escaping needed:** because the `.razor.hs` file is plain text rather than Razor markup, hyperscript attributes that begin with `@` (e.g., `@hidden`, `@data-items`) are written naturally with a single `@`.

**No build-time validation:** the generator emits file content verbatim into a raw-string literal. Hyperscript parse errors surface at element initialisation in DevTools, not at build time.

**Example:** `Features/Tv/Controls/AlternativeTitlesEditor.razor.hs` in `AnimeFeedManager.Web` produces `partial class AlternativeTitlesEditor` in namespace `AnimeFeedManager.Web.Features.Tv.Controls`.

**Consumer wiring required** in addition to the project reference:

```xml
<ItemGroup>
  <AdditionalFiles Include="**\*.razor.hs" />
</ItemGroup>
<ItemGroup>
  <CompilerVisibleProperty Include="RootNamespace" />
  <CompilerVisibleItemMetadata Include="AdditionalFiles" MetadataName="RelativeDir" />
</ItemGroup>
```

---

### QueueNames

Aggregates queue-name constants from all classes and records that inherit from `DomainMessage` into a single generated static class.

| Detail | Value |
|---|---|
| Trigger | Types inheriting from `DomainMessage` with `const string` fields whose name contains "queue" (case-insensitive) |
| Output namespace | `AnimeFeedManager.Generated` |
| Key emitted symbol | `DomainMessageQueues` (static class) |
| Consumer wiring | None beyond the project reference |
| Diagnostics | None |

**Emitted members of `DomainMessageQueues`:**

- One `const string` per discovered queue field, named after its source type.
- `AllQueues` — `string[]` containing every queue name.
- `AllQueuesSpan` — `ReadOnlySpan<string>` over the same values.
- `QueueCount` — `const int`.

---

### TableNames

Synthesizes an internal `WithTableNameAttribute` for marking Azure Table Storage entity classes. Finds classes that both implement `ITableEntity` and carry that attribute, then emits a name-to-type lookup.

| Detail | Value |
|---|---|
| Trigger | Class decorated with `[WithTableName]` |
| Required interface | `ITableEntity` |
| Output | Table name lookup map |
| Consumer wiring | None beyond the project reference |

**Diagnostics**

| ID | Condition |
|---|---|
| `TNM001` | Decorated class does not implement `ITableEntity` |

---

### Validation

Emits flat-tuple `And<T>` extension methods for `Validation<T>` chaining — sizes 2 through 7 — allowing `validation.And(other).And(more)` call chains with accumulating tuple types up to 7-tuples.

| Detail | Value |
|---|---|
| Trigger | Assembly name equals `AnimeFeedManager.Shared` |
| Output namespace | `AnimeFeedManager.Shared.Results.Static` |
| Consumer wiring | None beyond the project reference |
| Diagnostics | None |

**Assembly-scoped:** the generator only emits when `compilation.AssemblyName == "AnimeFeedManager.Shared"`, because the emitted code depends on internal members of `Validation<T>` defined in that assembly.

---

## Adding a New Generator

1. Create a new folder `<Name>/` under the project root.
2. Add `Generator.cs` in that folder containing a class named `<Name>SourceGenerator` (match the naming pattern of existing generators) implementing `IIncrementalGenerator` with the `[Generator]` attribute.
3. Add a section for the new generator to this README, in alphabetical order with the existing entries.
