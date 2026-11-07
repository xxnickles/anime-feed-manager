namespace AnimeFeedManager.Features.Tv.FeedManager;

/// <summary>Spreads bulk sends that each lead to a nyaa search: groups of five, ten seconds apart.</summary>
public static class NyaaPacing
{
    private const int GroupSize = 5;
    private static readonly TimeSpan GroupInterval = TimeSpan.FromSeconds(10);

    public static Delay For(int index) => new(GroupInterval * (index / GroupSize));
}
