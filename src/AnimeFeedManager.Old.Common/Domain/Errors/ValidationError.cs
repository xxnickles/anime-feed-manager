using System.Text;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Domain.Errors;

public sealed class ValidationError(string field, string[] errors)
{
    public KeyValuePair<string, string[]> Error { get; } = new(field, errors);

    public static ValidationError Create(string field, string[] errors) => new(field, errors);
    public static ValidationError Create(string field, string error) => new(field, [error]);
}

public class ValidationErrors : DomainError
{
    public ImmutableDictionary<string, string[]> Errors { get; }

    private ValidationErrors(IEnumerable<ValidationError> errors)
        : base("One or more validations have failed")
    {
        Errors = new Dictionary<string, string[]>(errors.Select(x => x.Error))
            .ToImmutableDictionary();
    }

    public static ValidationErrors Create(IEnumerable<ValidationError> errors) =>
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
            logger.LogWarning("Field: {Field} Messages: {Messages}", validationError.Key,
                string.Join(". ", validationError.Value));
    }
}