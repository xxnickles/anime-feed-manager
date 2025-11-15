using System.Text;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record DomainValidationError(string Field, string[] Errors)
{
    public KeyValuePair<string, string[]> Error { get; } = new(Field, Errors);

    public static DomainValidationError Create(string field, string[] errors) => new(field, errors);
    public static DomainValidationError Create(string field, string error) => new(field, [error]);

    public static DomainValidationError Create<T>(string[] errors) => Create(typeof(T).Name, errors);
    public static DomainValidationError Create<T>(string error) => Create(typeof(T).Name, [error]);
}

public sealed record DomainValidationErrors : DomainError
{
    public List<DomainValidationError> Errors { get; set; } = [];

    private DomainValidationErrors(IEnumerable<DomainValidationError> errors,
        string CallerMemberName,
        string CallerFilePath,
        int CallerLineNumber)
        : base("One or more validations have failed", CallerMemberName, CallerFilePath, CallerLineNumber)
    {
        Errors.AddRange(errors);
    }

    public static DomainValidationErrors Create(IEnumerable<DomainValidationError> errors,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) =>
        new(errors, callerMemberName, callerFilePath, callerLineNumber);

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

    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogWarning("{Error}", Message);

        foreach (var validationError in Errors)
            logger.LogWarning("Field: {Field} Messages: {Messages}", validationError.Field,
                string.Join(". ", validationError.Errors));
    }
}

// C# 14 Extension members for DomainValidationErrors and DomainValidationError
public static class DomainValidationErrorExtensions
{
    extension(DomainValidationErrors validationErrors)
    {
        public DomainValidationErrors AppendErrors(DomainValidationErrors otherValidationErrors)
        {
            validationErrors.Errors.AddRange(otherValidationErrors.Errors);
            return validationErrors;
        }
    }

    extension(DomainValidationError validationError)
    {
        public DomainValidationErrors ToErrors() =>
            DomainValidationErrors.Create([validationError]);
    }

    extension(IEnumerable<DomainValidationError> validationErrors)
    {
        public DomainValidationErrors ToErrors() =>
            DomainValidationErrors.Create(validationErrors);
    }
}