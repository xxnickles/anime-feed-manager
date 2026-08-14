using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace AnimeFeedManager.Web.Features.NotificationEmail;

/// <summary>Hosts an arbitrary RenderFragment so HtmlRenderer has a single component type to render.</summary>
public sealed class WrapperComponent : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, ChildContent);
}
