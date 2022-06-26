using AnimeFeedManager.Core.Error;
using LanguageExt;

namespace AnimeFeedManager.Core.Utils;

public static class LanguageExtensions
{
    public static Either<DomainError, TR> ToEither<TR>(this Validation<ValidationError, TR> validation, string correlationId) =>
        validation.ToEither().MapLeft(errors => (DomainError)ValidationErrors.Create(correlationId, errors));
}