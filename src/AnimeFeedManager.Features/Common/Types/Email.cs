using System.Text.RegularExpressions;

namespace AnimeFeedManager.Features.Common.Types;

public partial record Email
{
    public readonly string Value;

    private Email(string value)
    {
        Value = value;
    }

    private static bool IsEmail(string value) => LocalRegex().Match(value).Success;

    public static Option<Email> FromString(string value) => IsEmail(value) switch
    {
        true => Some(new Email(value)),
        false => None
    };

    [GeneratedRegex(
        @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?")]
    private static partial Regex LocalRegex();

    public static implicit operator string(Email email) => email.Value;
}

public static class EmailValidator
{
    public static Validation<ValidationError, Email> Validate(string emailValue)
    {
        return Email.FromString(emailValue).ToValidation(
                ValidationError.Create("Email", new[] { "A valid email address must be provided" }));
    }

}