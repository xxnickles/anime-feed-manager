namespace AnimeFeedManager.Features.Users.Types;

public record UserRegistration(Email Email, UserId UserId, NoEmptyString DisplayName);

public abstract record UsersCheck;

public record AllMatched : UsersCheck;

public record SomeNotFound(ImmutableList<string> NotFoundUsers): UsersCheck;