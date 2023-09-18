using AnimeFeedManager.Features.Common.Domain.Errors;

namespace AnimeFeedManager.Features.Common.Types;

public record UserId
{
    public readonly string Value;

    private UserId(string value)
    {
        Value = value;
    }

    public static Option<UserId> FromString(string value) => !string.IsNullOrWhiteSpace(value) switch
    {
        true => Some(new UserId(value)),
        false => None
    };

    public static implicit operator string(UserId userId) => userId.Value;
}

public static class UserIdValidator
{
    public static Validation<ValidationError, UserId> Validate(string userId)
    {
        return UserId.FromString(userId).ToValidation(
                ValidationError.Create("UserId", new[] { "User Id cannot be en empty string" }));
    }
}