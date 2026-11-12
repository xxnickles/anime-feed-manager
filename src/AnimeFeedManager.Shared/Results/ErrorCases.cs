namespace AnimeFeedManager.Shared.Results;

internal static class ErrorCases
{
    /// <summary>The DomainError behind an error: itself for a leaf or family, the contents for a union.</summary>
    /// <exception cref="InvalidOperationException">The error is neither a DomainError nor a union of them.</exception>
    internal static DomainError AsDomainError<TError>(TError error) => error switch
    {
        DomainError domainError => domainError,
        IUnion { Value: DomainError domainError } => domainError,
        _ => throw new InvalidOperationException(
            $"{typeof(TError).Name} is not a DomainError or a union of DomainErrors")
    };
}
