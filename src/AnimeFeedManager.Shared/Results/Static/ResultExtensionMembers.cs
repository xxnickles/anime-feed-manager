namespace AnimeFeedManager.Shared.Results.Static;

public static class ResultExtensionMembers
{
    extension<T>(Result<T> result)
    {
        public void Match(Action<T> onOk, Action<DomainError> onError)
        {
            if (result.IsSuccess)
                onOk(result.ResultValue!);
            else
                onError(result.ErrorValue!);
        }
        
        public async Task Match(Func<T, Task> onOk, Func<DomainError, Task> onError)
        {
            if (result.IsSuccess)
                await onOk(result.ResultValue!);
            else
                await onError(result.ErrorValue!);
        }
        
        public Result<TTarget> Map<TTarget>(Func<T, TTarget> mapper) =>
            result.IsSuccess
                ? new Result<TTarget>(true, mapper(result.ResultValue!), null)
                : new Result<TTarget>(false, default, result.ErrorValue);

        public async Task<Result<TTarget>> Map<TTarget>(Func<T, Task<TTarget>> mapper) =>
            result.IsSuccess
                ? new Result<TTarget>(true, await mapper(result.ResultValue!), null)
                : new Result<TTarget>(false, default, result.ErrorValue);

        public TTarget MatchToValue<TTarget>(Func<T, TTarget> onOk, Func<DomainError, TTarget> onError) =>
            result.IsSuccess
                ? onOk(result.ResultValue!)
                : onError(result.ErrorValue!);

        public Result<T> MapError(Func<DomainError, DomainError> mapper) =>
            result.IsSuccess
                ? new Result<T>(true, result.ResultValue, null)
                : new Result<T>(false, default, mapper(result.ErrorValue!));

        public async Task<Result<T>> MapError(Func<DomainError, Task<DomainError>> mapper) =>
            result.IsSuccess
                ? new Result<T>(true, result.ResultValue, null)
                : new Result<T>(false, default, await mapper(result.ErrorValue!));


        public Result<TTarget> Bind<TTarget>(Func<T, Result<TTarget>> binder) =>
            result.IsSuccess ? binder(result.ResultValue!) : new Result<TTarget>(false, default, result.ErrorValue);

        public async Task<Result<TTarget>> Bind<TTarget>(Func<T, Task<Result<TTarget>>> binder) =>
            result.IsSuccess ? await binder(result.ResultValue!) : new Result<TTarget>(false, default, result.ErrorValue);
    }
    
   
}