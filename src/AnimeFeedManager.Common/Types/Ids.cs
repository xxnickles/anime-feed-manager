using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Utils;

namespace AnimeFeedManager.Common.Types;

public abstract record DomainId
{
    public readonly string Value;

    protected DomainId(string value)
    {
        Value = value;
    }

    public static implicit operator string(DomainId userId) => userId.Value;

    public override string ToString()
    {
        return Value;
    }
}

public record UserId : DomainId
{
    private UserId(string value) : base(value)
    {
    }

    public static Option<UserId> FromString(string value) => !string.IsNullOrWhiteSpace(value) switch
    {
        true => Some(new UserId(value)),
        false => None
    };

    public static Validation<ValidationError, UserId> Validate(string id)
    {
        return FromString(id).ToValidation(
            ValidationError.Create(nameof(UserId), ["User Id cannot be en empty string"]));
    }

    public static Either<DomainError, UserId> Parse(string id)
    {
        return Validate(id)
            .ValidationToEither();
    }
}

public record PartitionKey : DomainId
{
    private PartitionKey(string value) : base(value)
    {
    }

    public static Option<PartitionKey> FromString(string value) => !string.IsNullOrWhiteSpace(value) switch
    {
        true => Some(new PartitionKey(value)),
        false => None
    };

    public static Validation<ValidationError, PartitionKey> Validate(string id)
    {
        return FromString(id).ToValidation(
            ValidationError.Create(nameof(PartitionKey), ["Partition Key cannot be en empty string"]));
    }

    public static Either<DomainError, PartitionKey> Parse(string id)
    {
        return Validate(id)
            .ValidationToEither();
    }
}

public record RowKey : DomainId
{
    private RowKey(string value) : base(value)
    {
    }

    public static Option<RowKey> FromString(string value) => !string.IsNullOrWhiteSpace(value) switch
    {
        true => Some(new RowKey(value)),
        false => None
    };

    public static Validation<ValidationError, RowKey> Validate(string id)
    {
        return FromString(id).ToValidation(
            ValidationError.Create(nameof(RowKey), ["Row Key cannot be en empty string"]));
    }

    public static Either<DomainError, RowKey> Parse(string id)
    {
        return Validate(id)
            .ValidationToEither();
    }
}