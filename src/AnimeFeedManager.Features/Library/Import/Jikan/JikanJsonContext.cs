using AnimeFeedManager.Features.Library.Import.Jikan.Types;

namespace AnimeFeedManager.Features.Library.Import.Jikan;

[JsonSerializable(typeof(JikanSeasonResponse))]
[JsonSerializable(typeof(JikanAnime))]
[JsonSerializable(typeof(JikanStreamingResponse))]
internal partial class JikanJsonContext : JsonSerializerContext;
