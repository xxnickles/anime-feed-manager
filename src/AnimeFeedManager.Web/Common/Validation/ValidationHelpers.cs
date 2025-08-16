using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AnimeFeedManager.Web.Common.Validation;

internal static class ValidationHelpers
{
    internal static Result<T> Validate<T>([NotNull] T value) where T : notnull
    {
        var validationIssues = new List<ValidationResult>();
        var context = new ValidationContext(value, serviceProvider: null, items: null);
        var t = Validator.TryValidateObject(value, context, validationIssues, validateAllProperties: true);

        return Validator.TryValidateObject(value, context, validationIssues, validateAllProperties: true)
            ? value
            : new FormDataValidationError<T>(validationIssues);
    }  

}