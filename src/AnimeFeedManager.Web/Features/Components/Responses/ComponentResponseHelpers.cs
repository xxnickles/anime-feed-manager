using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Components.Responses;

internal static class ComponentResponseHelpers
{
    internal static RazorComponentResult AggregateComponents(this RenderFragment[] fragments,
        int statusCode = StatusCodes.Status500InternalServerError)
    {
        return new RazorComponentResult<FragmentContainer>(new Dictionary<string, object?>
        {
            {nameof(FragmentContainer.ChildContent), Combine(fragments)}
        })
        {
            StatusCode = statusCode
        };
    }

    private static RenderFragment Combine(RenderFragment[] fragments) => builder =>
    {
        for (var i = 0; i < fragments.Length; i++)
        {
            builder.AddContent(0, fragments[i]);
        }
    };
}