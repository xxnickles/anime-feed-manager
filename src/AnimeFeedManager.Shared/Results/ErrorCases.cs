namespace AnimeFeedManager.Shared.Results;

internal static class ErrorCases
{
    /// <summary>The DomainError behind an error: itself for a leaf or family, the contents for a union, nested unions included.</summary>
    /// <exception cref="InvalidOperationException">The error is neither a DomainError nor a union of them.</exception>
    internal static DomainError AsDomainError<TError>(TError error) =>
        Unwrap(error) ?? throw new InvalidOperationException(
            $"{typeof(TError).Name} is not a DomainError or a union of DomainErrors");

    private static DomainError? Unwrap(object? error) => error switch
    {
        DomainError domainError => domainError,
        IUnion { Value: var value } => Unwrap(value),
        _ => null
    };
}
