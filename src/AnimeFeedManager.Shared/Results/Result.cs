namespace AnimeFeedManager.Shared.Results;

public readonly record struct Unit;

// ReSharper disable NullableWarningSuppressionIsUsed
public sealed record Result<T>
{
    internal T? ResultValue { get; init; }
    internal DomainError? ErrorValue { get; init; }
    internal bool IsSuccess { get; init; }
    internal TraceContext TraceContext { get; init; }

    internal Result(T? resultValue, DomainError? errorValueValue, bool isSuccess, 
        TraceContext? traceContext = null) =>
        (ResultValue, ErrorValue,IsSuccess, TraceContext) = (resultValue, errorValueValue,isSuccess, traceContext ?? TraceContext.Empty);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(DomainError error) => Failure(error);

    public static Result<Unit> Success() => new Unit();

    public static Result<T> Success(T value) => new(value, null,true);

    public static Result<T> Failure(DomainError error) => new(default, error,false);
}