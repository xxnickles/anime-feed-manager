using Passwordless;
using Passwordless.Models;

namespace AnimeFeedManager.Features.User.Authentication.RegistrationProcess;

public sealed record RegistrationProcessData(
    NewRegistration Registration,
    Authentication.Storage.User StorageUser,
    RegisterOptions Options);

public sealed record UserRegistrationResult(Email Email, string UserId, RegisterTokenResponse Token);
