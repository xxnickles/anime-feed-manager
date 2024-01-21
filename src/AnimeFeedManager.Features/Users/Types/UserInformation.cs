namespace AnimeFeedManager.Features.Users.Types;

public record UserRegistration(Email Email, UserId UserId, NoEmptyString DisplayName);
