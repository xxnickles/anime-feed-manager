using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Core.Utils;

public static class EmailExtensions
{
    public static Validation<ValidationError, Email> ToValidation(this Email email, ValidationError error)
    {
        var value = email.Value;
        return value.Match(
            _ => Success<ValidationError, Email>(email),
            () => Fail<ValidationError, Email>(error));
    }
}