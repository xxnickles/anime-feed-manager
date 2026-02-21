namespace AnimeFeedManager.Features.Common;

[JsonSerializable(typeof(SeriesSeason[]))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class CommonJsonContext : JsonSerializerContext;
