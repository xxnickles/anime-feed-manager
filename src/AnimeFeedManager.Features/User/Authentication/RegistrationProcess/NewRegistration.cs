namespace AnimeFeedManager.Features.User.RegistrationProcess;

public record NewRegistration(Email Email, string UserId, NoEmptyString DisplayName);

public static class UserRegistrationExtensions
{
    public static Result<NewRegistration> AsUserRegistration(this (string email, string displayName) target)
    {
        return target.email.ParseAsEmail()
            .And(target.displayName.ParseAsNonEmpty())
            .Map(result => new NewRegistration(result.Item1, IdHelpers.GetUniqueId(), result.Item2));
    }
}