using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Common;

internal interface INotifiableComponent<in T> where T : class, new()
{
    // public T ViewModel { get; set; }
    public static abstract RenderFragment AsRenderFragment(T viewModel);
    public static abstract RenderFragment OkNotificationContent(T viewModel);

    public  static abstract string SuccessNotificationTitle { get; }

    public static abstract string ErrorNotificationTitle { get; }
}