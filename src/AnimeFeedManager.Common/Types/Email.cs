using System.Text.RegularExpressions;
using AnimeFeedManager.Common.Domain.Errors;

namespace AnimeFeedManager.Common.Types;

public partial record Email
{
    private readonly string _value;

    private Email(string value)
    {
        _value = value;
    }

    public static bool IsEmail(string value) => LocalRegex().Match(value).Success;

    public static Option<Email> FromString(string value) => IsEmail(value) switch
    {
        true => Some(new Email(value)),
        false => None
    };

    [GeneratedRegex(
        @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?")]
    private static partial Regex LocalRegex();

    public static implicit operator string(Email email) => email._value;

    public override string ToString()
    {
        return _value;
    }
}

public static class EmailValidator
{
    public static Validation<ValidationError, Email> Validate(string emailValue)
    {
        return Email.FromString(emailValue).ToValidation(
            ValidationError.Create("Email", ["A valid email address must be provided"]));
    }

}