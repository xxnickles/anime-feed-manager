using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons.Types;

namespace AnimeFeedManager.Features.Library;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> exposing the Library
/// feature's serializable types. Register alongside other feature contexts
/// when wiring the Cosmos serializer. Options here mirror those configured in
/// <c>AddCosmosInfrastructure</c> so direct uses of <c>LibraryJsonContext.Default.*</c>
/// (e.g. stream-based upserts that bypass the Cosmos serializer) produce JSON
/// identical to what Cosmos would write through its own pipeline.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Series))]
[JsonSerializable(typeof(TvSeries))]
[JsonSerializable(typeof(MovieSeries))]
[JsonSerializable(typeof(OvaSeries))]
[JsonSerializable(typeof(OnaSeries))]
[JsonSerializable(typeof(TvSpecialSeries))]
[JsonSerializable(typeof(LibrarySeasonsIndex))]
[JsonSerializable(typeof(SeasonEntry))]
public partial class LibraryJsonContext : JsonSerializerContext;
