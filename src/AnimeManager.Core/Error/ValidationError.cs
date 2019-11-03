using System.Collections.Generic;
using System.Collections.Immutable;

namespace AnimeFeedManager.Core.Error
{
    public class ValidationError
    {
        public string Field { get; }
        public string Description { get; }

        public ValidationError(string field, string description)
        {
            Field = field;
            Description = description;
        }

        public static ValidationError Create(string source, string error) => new ValidationError(source,error);
    }

    public class ValidationErrors : DomainError
    {
        public ImmutableList<ValidationError> Errors { get; }

        public ValidationErrors(string correlationId, IEnumerable<ValidationError> errors)
            : base(correlationId, "One or more validations have failed")
        {
            Errors = errors.ToImmutableList();
        }

        public static ValidationErrors Create(string correlationId, IEnumerable<ValidationError> errors) => new ValidationErrors(correlationId, errors);
    }
}
