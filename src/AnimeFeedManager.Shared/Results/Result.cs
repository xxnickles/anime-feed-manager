namespace AnimeFeedManager.Shared.Results;

public readonly record struct Unit;

// ReSharper disable NullableWarningSuppressionIsUsed
public sealed record Result<T>
{
    internal T? ResultValue { get; }
    internal  DomainError? ErrorValue { get; }
    internal bool IsSuccess { get; }

    internal Result(bool isSuccess, T? resultValue, DomainError? errorValueValue) =>
        (IsSuccess, ResultValue, ErrorValue) = (isSuccess, resultValue, errorValueValue);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(DomainError error) => Failure(error);

    public static Result<Unit> Success() => new Unit();
    
    public static Result<T> Success(T value) => new (true, value, null);
    
    public static Result<T> Failure(DomainError error) => new (false, default, error);
}