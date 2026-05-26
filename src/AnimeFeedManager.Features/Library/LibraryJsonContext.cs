using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons.Types;

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
[JsonSerializable(typeof(LibrarySeasonsIndex))]
[JsonSerializable(typeof(SeasonEntry))]
internal partial class LibraryJsonContext : JsonSerializerContext;
