namespace AnimeFeedManager.Shared.Results;

public readonly record struct Unit;

/// <summary>A success value or an error; a custom union of both that also carries the deferred trace context.</summary>
// ReSharper disable NullableWarningSuppressionIsUsed
[Union]
public sealed record Result<T, TError> : Result<T, TError>.IUnionMembers
{
    internal T? ResultValue { get; }
    internal TError? ErrorValue { get; }
    internal bool IsSuccess { get; }
    internal TraceContext TraceContext { get; init; } = TraceContext.Empty;

    private Result(T value) => (ResultValue, IsSuccess) = (value, true);
    private Result(TError error) => ErrorValue = error;

    // Union member provider: Create defines the cases and the implicit conversions from T and TError.
    public interface IUnionMembers
    {
        static Result<T, TError> Create(T value) => new(value);
        static Result<T, TError> Create(TError error) => new(error);
        object? Value { get; }
        bool HasValue { get; }
        bool TryGetValue(out T value);
        bool TryGetValue(out TError error);
    }

    // Boxes a value-type T; pattern matching uses TryGetValue instead.
    object? IUnionMembers.Value => IsSuccess ? ResultValue : ErrorValue;

    public bool HasValue => IsSuccess ? ResultValue is not null : ErrorValue is not null;

    public bool TryGetValue(out T value)
    {
        value = ResultValue!;
        return IsSuccess;
    }

    public bool TryGetValue(out TError error)
    {
        error = ErrorValue!;
        return !IsSuccess;
    }

    public static Result<T, TError> Success(T value) => new(value);

    public static Result<T, TError> Failure(TError error) => new(error);

    internal static Result<T, TError> Success(T value, TraceContext traceContext) =>
        new(value) { TraceContext = traceContext };

    internal static Result<T, TError> Failure(TError error, TraceContext traceContext) =>
        new(error) { TraceContext = traceContext };
}
