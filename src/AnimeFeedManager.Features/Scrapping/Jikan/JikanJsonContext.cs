namespace AnimeFeedManager.Features.Scrapping.Jikan;

[JsonSerializable(typeof(JikanSeasonResponse))]
[JsonSerializable(typeof(JikanAnime))]
[JsonSerializable(typeof(JikanPagination))]
[JsonSerializable(typeof(JikanImages))]
[JsonSerializable(typeof(JikanImagesJpg))]
[JsonSerializable(typeof(JikanAired))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class JikanJsonContext : JsonSerializerContext;
