using System.ComponentModel.DataAnnotations;
using AnimeFeedManager.Shared.Types;

namespace AnimeFeedManager.Web.Common.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class AsSeason : ValidationAttribute
{
    public AsSeason()
    {
        // Set a default error message that can be accessed without validation
        ErrorMessage = "{0} is not a valid season.";
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessageString, name, ActualValue?.ToString() ?? name);
    }

    private object? ActualValue { get; set; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null || (value is string stringValue && Season.IsValid(stringValue)))
            return ValidationResult.Success;

        ActualValue = value; // Store the actual value for error formatting

        var errorMessage = FormatErrorMessage(validationContext.DisplayName);
        return new ValidationResult(errorMessage, validationContext.MemberName is not null ? [validationContext.MemberName] : null);
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class AsYear : ValidationAttribute
{
    public AsYear()
    {
        // Set a default error message that can be accessed without validation
        ErrorMessage = "{0} is not a valid year.";
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessageString, name, ActualValue?.ToString() ?? name);
    }

    private object? ActualValue { get; set; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null || (value is int intValue && Year.NumberIsValid(intValue)))
            return ValidationResult.Success;

        ActualValue = value; // Store the actual value for error formatting

        var errorMessage = FormatErrorMessage(validationContext.DisplayName);
        return new ValidationResult(errorMessage, validationContext.MemberName is not null ? [validationContext.MemberName] : null);
    }

}