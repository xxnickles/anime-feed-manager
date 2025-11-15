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

// C# 14 Extension members for Validation<T>
public static class ValidationExtensions
{
    // Basic combinator for single value validations
    extension<T>(Validation<T> validation)
    {
        public Validation<(T, T2)> And<T2>(Validation<T2> other)
        {
            if (validation.IsSuccess && other.IsSuccess)
                return Validation<(T, T2)>.Valid((validation.ValidValue, other.ValidValue));

            if (!validation.IsSuccess && !other.IsSuccess)
                return Validation<(T, T2)>.Invalid(validation.ErrorValue.AppendErrors(other.ErrorValue));

            return !validation.IsSuccess
                ? Validation<(T, T2)>.Invalid(validation.ErrorValue)
                : Validation<(T, T2)>.Invalid(other.ErrorValue);
        }
    }

    // Extension members for flat tuple of size 2 adding a third element
    extension<TA, TB>(Validation<(TA, TB)> validation)
    {
        public Validation<(TA, TB, TC)> And<TC>(Validation<TC> other)
        {
            return validation.IsSuccess switch
            {
                true when other.IsSuccess => Validation<(TA, TB, TC)>.Valid((validation.ValidValue.Item1,
                    validation.ValidValue.Item2, other.ValidValue)),
                false when !other.IsSuccess => Validation<(TA, TB, TC)>.Invalid(
                    validation.ErrorValue.AppendErrors(other.ErrorValue)),
                _ => !validation.IsSuccess
                    ? Validation<(TA, TB, TC)>.Invalid(validation.ErrorValue)
                    : Validation<(TA, TB, TC)>.Invalid(other.ErrorValue)
            };
        }
    }

    // Extension members for flat tuple of size 3 adding a fourth element
    extension<TA, TB, TC>(Validation<(TA, TB, TC)> validation)
    {
        public Validation<(TA, TB, TC, TD)> And<TD>(Validation<TD> other)
        {
            return validation.IsSuccess switch
            {
                true when other.IsSuccess => Validation<(TA, TB, TC, TD)>.Valid((validation.ValidValue.Item1,
                    validation.ValidValue.Item2, validation.ValidValue.Item3, other.ValidValue)),
                false when !other.IsSuccess => Validation<(TA, TB, TC, TD)>.Invalid(
                    validation.ErrorValue.AppendErrors(other.ErrorValue)),
                _ => !validation.IsSuccess
                    ? Validation<(TA, TB, TC, TD)>.Invalid(validation.ErrorValue)
                    : Validation<(TA, TB, TC, TD)>.Invalid(other.ErrorValue)
            };
        }
    }

    // Extension members for flat tuple of size 4 adding a fifth element
    extension<TA, TB, TC, TD>(Validation<(TA, TB, TC, TD)> validation)
    {
        public Validation<(TA, TB, TC, TD, TE)> And<TE>(Validation<TE> other)
        {
            return validation.IsSuccess switch
            {
                true when other.IsSuccess => Validation<(TA, TB, TC, TD, TE)>.Valid((validation.ValidValue.Item1,
                    validation.ValidValue.Item2, validation.ValidValue.Item3, validation.ValidValue.Item4,
                    other.ValidValue)),
                false when !other.IsSuccess => Validation<(TA, TB, TC, TD, TE)>.Invalid(
                    validation.ErrorValue.AppendErrors(other.ErrorValue)),
                _ => !validation.IsSuccess
                    ? Validation<(TA, TB, TC, TD, TE)>.Invalid(validation.ErrorValue)
                    : Validation<(TA, TB, TC, TD, TE)>.Invalid(other.ErrorValue)
            };
        }
    }

    // Extension members for flat tuple of size 5 adding a sixth element
    extension<TA, TB, TC, TD, TE>(Validation<(TA, TB, TC, TD, TE)> validation)
    {
        public Validation<(TA, TB, TC, TD, TE, TF)> And<TF>(Validation<TF> other)
        {
            return validation.IsSuccess switch
            {
                true when other.IsSuccess => Validation<(TA, TB, TC, TD, TE, TF)>.Valid((validation.ValidValue.Item1,
                    validation.ValidValue.Item2, validation.ValidValue.Item3, validation.ValidValue.Item4,
                    validation.ValidValue.Item5, other.ValidValue)),
                false when !other.IsSuccess => Validation<(TA, TB, TC, TD, TE, TF)>.Invalid(
                    validation.ErrorValue.AppendErrors(other.ErrorValue)),
                _ => !validation.IsSuccess
                    ? Validation<(TA, TB, TC, TD, TE, TF)>.Invalid(validation.ErrorValue)
                    : Validation<(TA, TB, TC, TD, TE, TF)>.Invalid(other.ErrorValue)
            };
        }
    }

    // Extension members for flat tuple of size 6 adding a seventh element
    extension<TA, TB, TC, TD, TE, TF>(Validation<(TA, TB, TC, TD, TE, TF)> validation)
    {
        public Validation<(TA, TB, TC, TD, TE, TF, TG)> And<TG>(Validation<TG> other)
        {
            return validation.IsSuccess switch
            {
                true when other.IsSuccess => Validation<(TA, TB, TC, TD, TE, TF, TG)>.Valid((
                    validation.ValidValue.Item1, validation.ValidValue.Item2, validation.ValidValue.Item3,
                    validation.ValidValue.Item4, validation.ValidValue.Item5, validation.ValidValue.Item6,
                    other.ValidValue)),
                false when !other.IsSuccess => Validation<(TA, TB, TC, TD, TE, TF, TG)>.Invalid(
                    validation.ErrorValue.AppendErrors(other.ErrorValue)),
                _ => !validation.IsSuccess
                    ? Validation<(TA, TB, TC, TD, TE, TF, TG)>.Invalid(validation.ErrorValue)
                    : Validation<(TA, TB, TC, TD, TE, TF, TG)>.Invalid(other.ErrorValue)
            };
        }
    }
}