using AnimeFeedManager.Features.Feeds.Sources.AniList.Types;

namespace AnimeFeedManager.Features.Feeds.Sources.AniList;

[JsonSerializable(typeof(AniListRequest))]
[JsonSerializable(typeof(AniListResponse))]
internal partial class AniListJsonContext : JsonSerializerContext;
