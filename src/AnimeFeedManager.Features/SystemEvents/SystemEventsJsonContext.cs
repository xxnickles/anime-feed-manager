namespace AnimeFeedManager.Features.SystemEvents;

[JsonSerializable(typeof(SystemEvent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class SystemEventsJsonContext : JsonSerializerContext;
