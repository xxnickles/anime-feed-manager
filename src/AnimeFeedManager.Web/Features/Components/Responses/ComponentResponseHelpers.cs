using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Components.Responses;

internal static class ComponentResponseHelpers
{
    internal static RazorComponentResult AggregateComponents(this RenderFragment[] fragments)
    {
        return new RazorComponentResult<AggregatorComponent>(new Dictionary<string, object?>
        {
            { nameof(AggregatorComponent.Fragments), fragments }
        });
    }

}