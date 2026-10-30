namespace AnimeFeedManager.Features.AppEvents;

[JsonSerializable(typeof(AppEvent))]
[JsonSerializable(typeof(AppEventEnvelope))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class AppEventsJsonContext : JsonSerializerContext;
