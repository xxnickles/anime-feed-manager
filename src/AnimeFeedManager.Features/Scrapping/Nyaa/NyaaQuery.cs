namespace AnimeFeedManager.Features.Scrapping.Nyaa;

public enum NyaaCategory
{
    EnglishTranslated
}

/// <summary>Immutable specification of a nyaa RSS request; each step returns a new query.</summary>
public sealed record NyaaQuery
{
    // Values are nyaa's f= codes.
    private enum NyaaFilter
    {
        None = 0,
        NoRemakes = 1,
        TrustedOnly = 2
    }

    private string Terms { get; init; } = string.Empty;
    private NyaaCategory Category { get; init; } = NyaaCategory.EnglishTranslated;
    private NyaaFilter Filter { get; init; } = NyaaFilter.None;

    private NyaaQuery()
    {
    }

    public static NyaaQuery Create() => new();

    public NyaaQuery Search(string terms) => this with {Terms = terms};

    public NyaaQuery InCategory(NyaaCategory category) => this with {Category = category};

    public NyaaQuery NoRemakes() => this with {Filter = NyaaFilter.NoRemakes};

    public NyaaQuery TrustedOnly() => this with {Filter = NyaaFilter.TrustedOnly};

    /// <summary>Request relative to the site root; <c>m=true</c> puts the magnet in each item's link.</summary>
    public string Build() =>
        $"?page=rss&m=true&c={CategoryCode(Category)}&f={(int) Filter}&q={Uri.EscapeDataString(Terms)}";

    private static string CategoryCode(NyaaCategory category) => category switch
    {
        NyaaCategory.EnglishTranslated => "1_2",
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };
}
