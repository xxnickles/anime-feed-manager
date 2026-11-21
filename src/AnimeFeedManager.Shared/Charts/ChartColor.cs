namespace AnimeFeedManager.Shared.Charts;

/// <summary>A daisyUI theme color; the client resolves <see cref="Token"/> to <c>var(--color-{Token})</c>.</summary>
/// <remarks>Closed set: the client allowlists these tokens, so add a member on both sides.</remarks>
public sealed record ChartColor
{
    public string Token { get; }

    private ChartColor(string token) => Token = token;

    public static ChartColor BaseContent { get; } = new("base-content");
    public static ChartColor Primary { get; } = new("primary");
    public static ChartColor Secondary { get; } = new("secondary");
    public static ChartColor Accent { get; } = new("accent");
    public static ChartColor Neutral { get; } = new("neutral");
    public static ChartColor Info { get; } = new("info");
    public static ChartColor Success { get; } = new("success");
    public static ChartColor Warning { get; } = new("warning");
    public static ChartColor Error { get; } = new("error");
}
