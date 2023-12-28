using Passwordless.Net;

namespace AnimeFeedManager.Features.Users.Types;

public record UserInformation(VerifiedUser VerifiedUser, Role Role);
