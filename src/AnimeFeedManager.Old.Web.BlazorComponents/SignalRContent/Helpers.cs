using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Web.BlazorComponents.Toast;

namespace AnimeFeedManager.Old.Web.BlazorComponents.SignalRContent;

internal static class  Helpers
{
    internal static ToastType Map(NotificationType type)
    {
        return type switch
        {
            NotificationType.Information => ToastType.Info,
            NotificationType.Update => ToastType.Success,
            NotificationType.Error => ToastType.Error,
            _ => ToastType.Warning
        };
    }
}