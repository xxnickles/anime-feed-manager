using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents;

public interface IRenderableComponent<in T> where T : class
{
    public static abstract RenderFragment AsRenderFragment(T viewModel);
}

public interface INotifiableComponent<in T> : IRenderableComponent<T> 
    where T : class, new() 
{
    public static abstract RenderFragment OkNotificationContent(T viewModel);

    public  static abstract string SuccessNotificationTitle { get; }

    public static abstract string ErrorNotificationTitle { get; }
}