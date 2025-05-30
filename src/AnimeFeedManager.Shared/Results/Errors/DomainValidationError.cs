using System.Text;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record DomainValidationError(string Field, string[] Errors)
{
    public KeyValuePair<string, string[]> Error { get; } = new(Field, Errors);

    public static DomainValidationError Create(string field, string[] errors) => new(field, errors);
    public static DomainValidationError Create(string field, string error) => new(field, [error]);
    
    public static DomainValidationError Create<T>(string[] errors) => Create(nameof(T), errors);
    public static DomainValidationError Create<T>(string error) => Create(nameof(T), [error]);
}

public sealed record DomainValidationErrors : DomainError
{
    public List<DomainValidationError> Errors { get; set; } = [];

    private DomainValidationErrors(IEnumerable<DomainValidationError> errors)
        : base("One or more validations have failed")
    {
        Errors.AddRange(errors);
    }

    public static DomainValidationErrors Create(IEnumerable<DomainValidationError> errors) =>
        new(errors);

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine(Message);
        builder.AppendLine("Validation Errors");
        foreach (var (key, value) in Errors)
        {
            builder.AppendLine($"{key}: {string.Join(", ", value)}");
        }

        return builder.ToString();
    }

    public override void LogError(ILogger logger)
    {
        logger.LogWarning("{Error}", Message);
        
        foreach (var validationError in Errors)
            logger.LogWarning("Field: {Field} Messages: {Messages}", validationError.Field,
                string.Join(". ", validationError.Errors));
    }
}

public static class Extensions
{
    public static DomainValidationErrors AppendErrors(this DomainValidationErrors validationErrors, DomainValidationErrors otherValidationErrors)
    {
        validationErrors.Errors.AddRange(otherValidationErrors.Errors);
        return validationErrors;
    }

    public static DomainValidationErrors ToErrors(this DomainValidationError validationError) =>
        DomainValidationErrors.Create([validationError]);
    
    public static DomainValidationErrors ToErrors(this IEnumerable<DomainValidationError> validationErrors) =>
        DomainValidationErrors.Create(validationErrors);
}