namespace AnimeFeedManager.Web.Features.Common.Toast;

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

internal static class Extensions
{
    internal static string AsStrokeStyle(this ToastType toastType)
    {
        return toastType switch
        {
            ToastType.Success => "stroke-success",
            ToastType.Error => "stroke-error",
            ToastType.Warning => "stroke-warning",
            ToastType.Info => "stroke-info",
            _ => "stroke-info"
        };
    }
}