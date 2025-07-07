using Passwordless;
using Passwordless.Models;

namespace AnimeFeedManager.Features.User.RegistrationProcess;

public sealed record RegistrationProcessData(
    NewRegistration Registration,
    Storage.User StorageUser,
    RegisterOptions Options);

public sealed record UserRegistrationResult(Email Email, string UserId, RegisterTokenResponse Token);
