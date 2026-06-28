using AnimeFeedManager.Features.Auth.Entities;

namespace AnimeFeedManager.Features.Auth;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> for the Auth feature's Cosmos
/// documents. Options mirror <c>AddCosmosInfrastructure</c> so stream-based reads/writes that
/// bypass the Cosmos serializer produce identical JSON. Registered alongside the other feature
/// contexts when wiring the Cosmos serializer.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(UserDocument))]
[JsonSerializable(typeof(UserAccount))]
[JsonSerializable(typeof(UsersIndex))]
[JsonSerializable(typeof(UserIndexEntry))]
public partial class AuthJsonContext : JsonSerializerContext;
