namespace AnimeFeedManager.Shared.Results;

/// <summary>A union that accepts <typeparamref name="TFamily"/>, and through <c>in</c> any subtype of it (a family's leaves); <c>Bind</c> widens a step's error through it.</summary>
public interface IWidensFrom<TSelf, in TFamily> where TSelf : IWidensFrom<TSelf, TFamily>
{
    static abstract TSelf Widen(TFamily error);
}
