using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public abstract record FormDataValidationError(
    ICollection<ValidationResult> ValidationResults,
    string CallerMemberName,
    string CallerFilePath,
    int CallerLineNumber) : DomainError("Form validation issues has been found", CallerMemberName, CallerFilePath,
    CallerLineNumber)
{
    protected abstract string GetTypeName();

    protected override void LoggingBehavior(ILogger logger)
    {
        var validationErrors = ValidationResults
            .Where(vr => !string.IsNullOrEmpty(vr.ErrorMessage))
            .Select(vr => new
            {
                ErrorMessage = vr.ErrorMessage,
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
    }
}

public sealed record FormDataValidationError<T>(
    ICollection<ValidationResult> ValidationResults,
    [CallerMemberName] string CallerMemberName = "",
    [CallerFilePath] string CallerFilePath = "",
    [CallerLineNumber] int CallerLineNumber = 0)
    : FormDataValidationError(ValidationResults, CallerMemberName, CallerFilePath, CallerLineNumber)
{
    protected override string GetTypeName() => typeof(T).Name;
}