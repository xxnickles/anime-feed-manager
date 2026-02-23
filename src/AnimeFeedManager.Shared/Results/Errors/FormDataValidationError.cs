using System.ComponentModel.DataAnnotations;

namespace AnimeFeedManager.Shared.Results.Errors;

public abstract record FormDataValidationError(
    ICollection<ValidationResult> ValidationResults) : DomainError("Form validation issues has been found")
{
    protected abstract string GetTypeName();

    public override Action<ILogger> LogAction() => logger =>
    {
        var validationErrors = ValidationResults
            .Where(vr => !string.IsNullOrEmpty(vr.ErrorMessage))
            .Select(vr => new
            {
                vr.ErrorMessage,
                MemberNames = vr.MemberNames?.ToArray() ?? Array.Empty<string>(),
                MemberNamesText = string.Join(", ", vr.MemberNames ?? Enumerable.Empty<string>())
            })
            .ToArray();

        var affectedFields = validationErrors
            .SelectMany(ve => ve.MemberNames)
            .Distinct()
            .ToArray();

        var typeName = GetTypeName();

        // Log with structured properties for easy searching
        logger.LogWarning(
            "Form validation failed for {ValidatedTypeName} with {ValidationErrorCount} error(s) on {AffectedFieldCount} field(s)",
            typeName,
            validationErrors.Length,
            affectedFields.Length);

        // Log each validation error separately for granular searching
        foreach (var error in validationErrors)
        {
            logger.LogWarning(
                "Validation error on {ValidatedTypeName}: {ValidationErrorMessage} for field(s): {AffectedFields}",
                typeName,
                error.ErrorMessage,
                error.MemberNamesText);
        }
    };

}

public sealed record FormDataValidationError<T>(
    ICollection<ValidationResult> ValidationResults)
    : FormDataValidationError(ValidationResults)
{
    protected override string GetTypeName() => typeof(T).Name;
}