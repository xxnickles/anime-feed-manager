using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Utils;

namespace AnimeFeedManager.Old.Common.Types;

public abstract record DomainId
{
    private readonly string _value;

    protected DomainId(string value)
    {
        _value = value;
    }

    public static implicit operator string(DomainId userId) => userId._value;

    public override string ToString()
    {
        return _value;
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

    public override string ToString()
    {
        return base.ToString();
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
    
    public override string ToString()
    {
        return base.ToString();
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
    
    public override string ToString()
    {
        return base.ToString();
    }
}