using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;

namespace AnimeFeedManager.Web.Features.Common;

internal class BlazorRenderer
{
    private readonly HtmlRenderer _htmlRenderer;

    public BlazorRenderer(HtmlRenderer htmlRenderer)
    {
        _htmlRenderer = htmlRenderer;
    }

    // Renders a component T which doesn't require any parameters
    public Task<RenderedComponent> RenderComponent<T>() where T : IComponent
        => RenderComponent<T>(ParameterView.Empty);

    // Renders a component T using the provided dictionary of parameters
    public Task<RenderedComponent> RenderComponent<T>(Dictionary<string, object?> dictionary) where T : IComponent
        => RenderComponent<T>(ParameterView.FromDictionary(dictionary));

    private Task<RenderedComponent> RenderComponent<T>(ParameterView parameters) where T : IComponent
    {
        // Use the default dispatcher to invoke actions in the context of the 
        // static HTML renderer and return as a string
        return _htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            HtmlRootComponent output = await _htmlRenderer.RenderComponentAsync<T>(parameters);
            return RenderedComponent.FromHtml(output.ToHtmlString());
        });
    }
}

internal record RenderedComponent(string Html)
{
    public static RenderedComponent FromHtml(string html) => new(html);

    internal RenderedComponent Combine(params RenderedComponent[] components)
    {
        return FromHtml(string.Concat(components.Select(x => x.Html).Concat([Html])));
    }
}