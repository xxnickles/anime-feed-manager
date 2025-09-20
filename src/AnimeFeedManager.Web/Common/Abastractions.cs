using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Common;

internal interface IRenderableComponent<in T> where T : class, new()
{
    public static abstract RenderFragment AsRenderFragment(T viewModel);
}

internal interface INotifiableComponent<in T> : IRenderableComponent<T> 
    where T : class, new() 
{
    public static abstract RenderFragment OkNotificationContent(T viewModel);

    public  static abstract string SuccessNotificationTitle { get; }

    public static abstract string ErrorNotificationTitle { get; }
}