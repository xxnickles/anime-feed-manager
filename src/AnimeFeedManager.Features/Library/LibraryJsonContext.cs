using AnimeFeedManager.Features.Library.Types;

namespace AnimeFeedManager.Features.Library;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> for the polymorphic
/// Series hierarchy. Register alongside <c>JikanJsonContext</c> when wiring
/// the Cosmos serializer.
/// </summary>
[JsonSerializable(typeof(Series))]
[JsonSerializable(typeof(TvSeries))]
[JsonSerializable(typeof(MovieSeries))]
[JsonSerializable(typeof(OvaSeries))]
[JsonSerializable(typeof(OnaSeries))]
[JsonSerializable(typeof(TvSpecialSeries))]
internal partial class SeriesJsonContext : JsonSerializerContext;
