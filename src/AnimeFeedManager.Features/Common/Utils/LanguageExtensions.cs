using AnimeFeedManager.Features.Domain.Errors;

namespace AnimeFeedManager.Features.Common.Utils;

public static class LanguageExtensions
{
    public static Either<DomainError, TR> ToEither<TR>(this Validation<ValidationError, TR> validation, string correlationId) =>
        validation.ToEither().MapLeft(errors => (DomainError)ValidationErrors.Create(correlationId, errors));
}