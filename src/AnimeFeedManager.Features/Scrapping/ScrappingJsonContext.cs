using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Scrapping;

[JsonSerializable(typeof(DailySeriesFeed[]))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class ScrappingJsonContext : JsonSerializerContext;
