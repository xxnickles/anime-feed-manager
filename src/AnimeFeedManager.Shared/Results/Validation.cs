using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results;

// ReSharper disable NullableWarningSuppressionIsUsed
public class Validation<T>
{
    private readonly T? _resultValue;
    private readonly DomainValidationErrors? _errorValueValue;
    private Validation(T? resultValue, DomainValidationErrors? errorValueValue)
    {
        _resultValue = resultValue;
        _errorValueValue = errorValueValue;
    }

    private bool IsSuccess => _errorValueValue is null;

    // Creator Methods
    public static Validation<T> Valid(T value) => new(value, null);
    public static Validation<T> Invalid(DomainValidationErrors error) => new(default, error);
    
    // Conversion
    
    public Validation<TTarget> Map<TTarget>(Func<T, TTarget> mapper) => IsSuccess ? 
        new Validation<TTarget>(mapper(_resultValue!), null) :
        new Validation<TTarget>(default, _errorValueValue!);
    
    
    private Result<T> ToResult()
    {
        return IsSuccess ? Result<T>.Success(_resultValue!) : Result<T>.Failure(_errorValueValue!);
    }

    public static implicit operator Result<T>(Validation<T> validation) => validation.ToResult();
    
    // Combinators

    public Validation<(T, T2)> And<T2>(Validation<T2> other) =>
        IsSuccess && other.IsSuccess            
            ? Validation<(T, T2)>.Valid((_resultValue, other._resultValue)!)
            : Validation<(T, T2)>.Invalid(_errorValueValue!.AppendErrors(other._errorValueValue!));
    
    public Validation<(T, T2, T3)> And<T2,T3>(Validation<(T2,T3)> other) =>
            IsSuccess && other.IsSuccess
                ? Validation<(T, T2, T3)>.Valid((_resultValue, other._resultValue.Item1, other._resultValue.Item2)!)
                : Validation<(T, T2, T3)>.Invalid(_errorValueValue!.AppendErrors(other._errorValueValue!));
    
    public Validation<(T, T2, T3, T4)> And<T2,T3,T4>(Validation<(T2,T3,T4)> other) =>
                IsSuccess && other.IsSuccess
                    ? Validation<(T, T2, T3, T4)>.Valid((_resultValue, other._resultValue.Item1, other._resultValue.Item2, other._resultValue.Item3)!)
                    : Validation<(T, T2, T3, T4)>.Invalid(_errorValueValue!.AppendErrors(other._errorValueValue!));
    
    
}