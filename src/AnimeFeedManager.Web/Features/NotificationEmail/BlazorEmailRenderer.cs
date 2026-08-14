using AnimeFeedManager.Features.Notifications;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AnimeFeedManager.Web.Features.NotificationEmail;

/// <summary>
/// Renders <see cref="NotificationDigestEmail"/> to an HTML string via <see cref="HtmlRenderer"/> —
/// the standard way to render a Razor component outside of a normal request. <see cref="HtmlRenderer"/>
/// is DI-registered scoped (framework construct, not a domain delegate), so this handler is built
/// per dispatch-job execution, never held by a singleton.
/// </summary>
public static class BlazorEmailRenderer
{
    public static NotificationEmailRenderer NotificationEmailRendererHandler(this HtmlRenderer renderer) =>
        (model, cancellationToken) => Render(renderer, model);

    private static async Task<Result<string>> Render(HtmlRenderer renderer, NotificationDigestView model)
    {
        try
        {
            var html = await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var output = await renderer.RenderComponentAsync<WrapperComponent>(
                    ParameterView.FromDictionary(new Dictionary<string, object?>
                    {
                        [nameof(WrapperComponent.ChildContent)] = ContentFor(model)
                    }));
                return output.ToHtmlString();
            });

            return html;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static RenderFragment ContentFor(NotificationDigestView model) => builder =>
    {
        builder.OpenComponent<NotificationDigestEmail>(0);
        builder.AddAttribute(1, nameof(NotificationDigestEmail.Model), model);
        builder.CloseComponent();
    };
}
