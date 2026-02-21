using AnimeFeedManager.Features.User.Authentication.Events;

namespace AnimeFeedManager.Features.User;

[JsonSerializable(typeof(UserRegistered))]
[EventPayloadSerializerContext(typeof(UserRegistered))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class UserJsonContext : JsonSerializerContext;
