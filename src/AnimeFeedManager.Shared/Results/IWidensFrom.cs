namespace AnimeFeedManager.Shared.Results;

/// <summary>A union that accepts <typeparamref name="TFamily"/>; <c>Bind</c> widens a step's error through it.</summary>
public interface IWidensFrom<TSelf, TFamily> where TSelf : IWidensFrom<TSelf, TFamily>
{
    static abstract TSelf Widen(TFamily error);
}
