namespace AnimeFeedManager.Shared.Results.Errors;

/// <summary>
/// Passthrough error that doesn't log anything
/// </summary>
public sealed record HandledError : DomainError
{
    private HandledError()
        : base(string.Empty)
    {
        
    }
    
    // Does nothing
    public override Action<ILogger> LogAction() => _ => { };
    
    public static HandledError Create() => new();
}