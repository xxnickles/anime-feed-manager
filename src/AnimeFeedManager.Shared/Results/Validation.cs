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

    // Conversion

    public Validation<TTarget> Map<TTarget>(Func<T, TTarget> mapper) => IsSuccess
        ? new Validation<TTarget>(true, mapper(ValidValue), null)
        : new Validation<TTarget>(false, default, ErrorValue);


    private Result<T> ToResult()
    {
        return IsSuccess ? ValidValue : ErrorValue;
    }

    public Result<T> AsResult() => ToResult();
    
    public static implicit operator Result<T>(Validation<T> validation) => validation.ToResult();
}

public static class ValidationExtensions
{
    // Basic combinator 
    public static Validation<(T, T2)> And<T, T2>(this Validation<T> validation, Validation<T2> other) =>
        validation.IsSuccess && other.IsSuccess
            ? Validation<(T, T2)>.Valid((validation.ValidValue, other.ValidValue))
            : Validation<(T, T2)>.Invalid(validation.ErrorValue.AppendErrors(other.ErrorValue));


    // Extension method for flat tuple of size 2 adding a third element
    public static Validation<(TA, TB, TC)>
        And<TA, TB, TC>(this Validation<(TA, TB)> validation, Validation<TC> other) =>
        validation.IsSuccess && other.IsSuccess
            ? Validation<(TA, TB, TC)>.Valid((validation.ValidValue.Item1, validation.ValidValue.Item2,
                other.ValidValue))
            : Validation<(TA, TB, TC)>.Invalid(validation.ErrorValue.AppendErrors(other.ErrorValue));

    // Extension method for flat tuple of size 3 adding a fourth element
    public static Validation<(TA, TB, TC, TD)> And<TA, TB, TC, TD>(this Validation<(TA, TB, TC)> validation,
        Validation<TD> other) =>
        validation.IsSuccess && other.IsSuccess
            ? Validation<(TA, TB, TC, TD)>.Valid((validation.ValidValue.Item1, validation.ValidValue.Item2,
                validation.ValidValue.Item3, other.ValidValue))
            : Validation<(TA, TB, TC, TD)>.Invalid(validation.ErrorValue.AppendErrors(other.ErrorValue));

    // Extension method for flat tuple of size 4 adding a fifth element
    public static Validation<(TA, TB, TC, TD, TE)> And<TA, TB, TC, TD, TE>(this Validation<(TA, TB, TC, TD)> validation,
        Validation<TE> other) =>
        validation.IsSuccess && other.IsSuccess
            ? Validation<(TA, TB, TC, TD, TE)>.Valid((validation.ValidValue.Item1, validation.ValidValue.Item2,
                validation.ValidValue.Item3, validation.ValidValue.Item4, other.ValidValue))
            : Validation<(TA, TB, TC, TD, TE)>.Invalid(validation.ErrorValue.AppendErrors(other.ErrorValue));

    // Extension method for flat tuple of size 5 adding a sixth element
    public static Validation<(TA, TB, TC, TD, TE, TF)> And<TA, TB, TC, TD, TE, TF>(
        this Validation<(TA, TB, TC, TD, TE)> validation, Validation<TF> other) =>
        validation.IsSuccess && other.IsSuccess
            ? Validation<(TA, TB, TC, TD, TE, TF)>.Valid((validation.ValidValue.Item1, validation.ValidValue.Item2,
                validation.ValidValue.Item3, validation.ValidValue.Item4, validation.ValidValue.Item5,
                other.ValidValue))
            : Validation<(TA, TB, TC, TD, TE, TF)>.Invalid(validation.ErrorValue.AppendErrors(other.ErrorValue));

    // Extension method for flat tuple of size 6 adding a seventh element
    public static Validation<(TA, TB, TC, TD, TE, TF, TG)> And<TA, TB, TC, TD, TE, TF, TG>(
        this Validation<(TA, TB, TC, TD, TE, TF)> validation, Validation<TG> other) =>
        validation.IsSuccess && other.IsSuccess
            ? Validation<(TA, TB, TC, TD, TE, TF, TG)>.Valid((validation.ValidValue.Item1,
                validation.ValidValue.Item2, validation.ValidValue.Item3, validation.ValidValue.Item4,
                validation.ValidValue.Item5, validation.ValidValue.Item6, other.ValidValue))
            : Validation<(TA, TB, TC, TD, TE, TF, TG)>.Invalid(validation.ErrorValue.AppendErrors(other.ErrorValue));
}