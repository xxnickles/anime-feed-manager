using AnimeFeedManager.Features.Library.Types;

namespace AnimeFeedManager.Features.Library;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> exposing the Library
/// feature's serializable types. Register alongside other feature contexts
/// when wiring the Cosmos serializer.
/// </summary>
[JsonSerializable(typeof(Series))]
[JsonSerializable(typeof(TvSeries))]
[JsonSerializable(typeof(MovieSeries))]
[JsonSerializable(typeof(OvaSeries))]
[JsonSerializable(typeof(OnaSeries))]
[JsonSerializable(typeof(TvSpecialSeries))]
internal partial class LibraryJsonContext : JsonSerializerContext;
