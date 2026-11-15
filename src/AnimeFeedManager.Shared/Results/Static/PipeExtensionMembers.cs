namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>Passes a value, or a task's value, to the next function, e.g. <c>Result&lt;T, TError&gt;.Success</c> to enter a chain.</summary>
public static class PipeExtensionMembers
{
    extension<T>(T value)
    {
        public TOut Pipe<TOut>(Func<T, TOut> next) => next(value);
    }

    // Not named Pipe: the any-T Pipe would also match the task itself, making generic method groups ambiguous.
    extension<T>(Task<T> task)
    {
        public async Task<TOut> Then<TOut>(Func<T, TOut> next) => next(await task);
    }
}
