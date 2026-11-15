using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results;

public class Validation<T>
{
    private readonly T? _validValue;
    private readonly ValidationError.Fields? _errorValue;

    internal T ValidValue => IsSuccess && _validValue is not null
        ? _validValue
        : throw new InvalidOperationException("Trying to access validation value when is not successful");

    internal ValidationError.Fields ErrorValue =>
        !IsSuccess && _errorValue is not null
            ? _errorValue
            : throw new InvalidOperationException("Trying to access validation error when is successful");


    private Validation(bool isSuccess, T? resultValue, ValidationError.Fields? errorValue) =>
        (IsSuccess, _validValue, _errorValue) = (isSuccess, resultValue, errorValue);


    internal bool IsSuccess { get; }

    public static Validation<T> Valid(T value) => new(true, value, null);
    public static Validation<T> Invalid(ValidationError.Fields error) => new(false, default, error);
    public static Validation<T> Invalid(FieldError error) => Invalid(new ValidationError.Fields([error]));

    /// <summary>Invalid, keyed by <c>typeof(T).Name</c> as the field name.</summary>
    public static Validation<T> Invalid(string message) => Invalid(new FieldError(typeof(T).Name, [message]));

    public Validation<TTarget> Map<TTarget>(Func<T, TTarget> mapper) =>
        IsSuccess
            ? Validation<TTarget>.Valid(mapper(ValidValue))
            : Validation<TTarget>.Invalid(ErrorValue);

    public Result<T, ValidationError.Fields> AsResult() => IsSuccess ? ValidValue : ErrorValue;

    public static implicit operator Result<T, ValidationError.Fields>(Validation<T> validation) => validation.AsResult();
}
