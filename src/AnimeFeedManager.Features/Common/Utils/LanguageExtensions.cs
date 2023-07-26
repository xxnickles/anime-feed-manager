namespace AnimeFeedManager.Features.Common.Utils;

public static class LanguageExtensions
{
    public static Either<DomainError, TR> ValidationToEither<TR>(this Validation<ValidationError, TR> validation) =>
        validation.ToEither().MapLeft(errors => (DomainError)ValidationErrors.Create(errors));
}