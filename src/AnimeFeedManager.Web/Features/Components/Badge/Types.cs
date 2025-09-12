namespace AnimeFeedManager.Web.Features.Components.Badge;

public enum StatusType
{
    Primary,
    Secondary,
    Accent,
    Neutral,
    Info,
    Success,
    Warning,
    Error
}

internal static class Constants
{
    internal static readonly Dictionary<StatusType, string> BadgeStylesMap = new()
    {
        {StatusType.Primary, "badge-primary"},
        {StatusType.Secondary, "badge-secondary"},
        {StatusType.Accent, "badge-accent"},
        {StatusType.Neutral, "badge-neutral"},
        {StatusType.Info, "badge-info"},
        {StatusType.Success, "badge-success"},
        {StatusType.Warning, "badge-warning"},
        {StatusType.Error, "badge-error"}
    };
}