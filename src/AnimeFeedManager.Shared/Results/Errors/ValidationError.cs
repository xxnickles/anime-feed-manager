using System.ComponentModel.DataAnnotations;

namespace AnimeFeedManager.Shared.Results.Errors;

/// <summary>One field's messages; <see cref="Field"/> is the input name, <c>typeof(T).Name</c> for a single-value parse.</summary>
public sealed record FieldError(string Field, ImmutableArray<string> Errors);

/// <summary>Client-input failures; edges route every leaf through the validation flow.</summary>
public closed record ValidationError(string Message) : DomainError(Message)
{
    public sealed record Fields(ImmutableArray<FieldError> Errors) : ValidationError("One or more validations have failed")
    {
        internal Fields Append(Fields other) => new([.. Errors, .. other.Errors]);

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(Message);
            builder.AppendLine("Validation Errors");
            foreach (var (field, errors) in Errors)
                builder.AppendLine($"{field}: {string.Join(", ", errors)}");

            return builder.ToString();
        }

        public override Action<ILogger> LogAction() => logger =>
        {
            logger.LogWarning("{domain_error.message}", Message);

            foreach (var (field, errors) in Errors)
                logger.LogWarning("Field: {validation.field} Messages: {validation.messages}", field,
                    string.Join(". ", errors));
        };
    }

    /// <summary>DataAnnotations failures on a posted form.</summary>
    public abstract record Form(ICollection<ValidationResult> ValidationResults)
        : ValidationError("Form validation issues has been found")
    {
        protected abstract string TypeName { get; }

        public override Action<ILogger> LogAction() => logger =>
        {
            var validationErrors = ValidationResults
                .Where(vr => !string.IsNullOrEmpty(vr.ErrorMessage))
                .Select(vr => (vr.ErrorMessage, MemberNames: vr.MemberNames?.ToArray() ?? []))
                .ToArray();

            var affectedFields = validationErrors
                .SelectMany(ve => ve.MemberNames)
                .Distinct()
                .Count();

            logger.LogWarning(
                "Form validation failed for {validation.type_name} with {validation.error_count} error(s) on {validation.field_count} field(s)",
                TypeName,
                validationErrors.Length,
                affectedFields);

            foreach (var (errorMessage, memberNames) in validationErrors)
                logger.LogWarning(
                    "Validation error on {validation.type_name}: {validation.messages} for field(s): {validation.field}",
                    TypeName,
                    errorMessage,
                    string.Join(", ", memberNames));
        };
    }

    public sealed record Form<T>(ICollection<ValidationResult> ValidationResults) : Form(ValidationResults)
    {
        protected override string TypeName => typeof(T).Name;
    }
}
