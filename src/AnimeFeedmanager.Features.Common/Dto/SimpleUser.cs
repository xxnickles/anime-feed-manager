using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Common.Dto;

public record SimpleUser(string UserId, string Email);

[JsonSerializable(typeof(SimpleUser))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SimpleUserContext : JsonSerializerContext
{
}