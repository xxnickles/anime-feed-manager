using AnimeFeedManager.Web.BlazorComponents.Toast;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent;

internal static class  Helpers
{
    internal static ToastType Map(EventType type)
    {
        return type switch
        {
            EventType.Information => ToastType.Info,
            EventType.Completed => ToastType.Success,
            EventType.Error => ToastType.Error,
            _ => ToastType.Warning
        };
    }
    
    
}