using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results;

public class Validation<T>
{
    private readonly T? _validValue;
    private readonly DomainValidationErrors? _errorValue;

    internal T ValidValue => IsSuccess && _validValue is not null
        ? _validValue
        : throw new InvalidOperationException("Trying to access validation value when is not successful");

    internal DomainValidationErrors ErrorValue =>
        !IsSuccess && _errorValue is not null
            ? _errorValue
            : throw new InvalidOperationException("Trying to access validation error when is successful");


    private Validation(bool isSuccess, T? resultValue, DomainValidationErrors? errorValue) =>
        (IsSuccess, _validValue, _errorValue) = (isSuccess, resultValue, errorValue);


    internal bool IsSuccess { get; }

    // Creator Methods
    public static Validation<T> Valid(T value) => new(true, value, null);
    public static Validation<T> Invalid(DomainValidationErrors error) => new(false, default, error);

    /// <summary>
    /// Creates an invalid validation using T as the field name.
    /// </summary>
    public static Validation<T> Invalid(string message) =>
        Invalid(DomainValidationError.Create<T>(message).ToErrors());

    /// <summary>
    /// Transforms the valid value using the provided mapper function.
    /// </summary>
    public Validation<TTarget> Map<TTarget>(Func<T, TTarget> mapper) =>
        IsSuccess
            ? Validation<TTarget>.Valid(mapper(ValidValue))
            : Validation<TTarget>.Invalid(ErrorValue);

    private Result<T> ToResult()
    {
        return IsSuccess ? ValidValue : ErrorValue;
    }

    public Result<T> AsResult() => ToResult();

    public static implicit operator Result<T>(Validation<T> validation) => validation.ToResult();
}
