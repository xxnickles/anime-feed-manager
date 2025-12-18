using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace AnimeFeedManager.Web.BlazorComponents;

/// <summary>
/// Wrapper component for rendering render fragments
/// </summary>
public class WrapperComponent: ComponentBase
{
    [Parameter] 
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }
} 
