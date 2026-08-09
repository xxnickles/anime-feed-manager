namespace AnimeFeedManager.Web.Features.Subscriptions;

/// <summary>
/// Subscribe/unsubscribe form. <c>Season</c> is bound leniently (as a string) so a tampered or
/// missing value flows through domain validation rather than failing minimal-API binding with a
/// raw 400 — mirrors <c>SeasonImportForm</c>. Both actions carry the full round-trip state
/// (<c>Season</c>, <c>Compact</c>) since either can be the button's next render.
/// </summary>
public sealed class SubscribeForm
{
    public int SeriesId { get; set; }
    public string? Season { get; set; }
    public bool Compact { get; set; }
}

public sealed class UnsubscribeForm
{
    public int SeriesId { get; set; }
    public string? Season { get; set; }
    public bool Compact { get; set; }
}
