using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Components.Responses;

/// <summary>
/// Builds an endpoint response out of several independently-rendered component fragments (e.g. a
/// component's next-state render plus an OOB toast) — one htmx response, several DOM effects.
/// </summary>
internal static class ComponentResponseHelpers
{
    internal static RenderFragment AsFragment<TComponent>(IReadOnlyDictionary<string, object?> parameters)
        where TComponent : IComponent =>
        builder =>
        {
            builder.OpenComponent<TComponent>(0);
            // ASP0006: sequence numbers are normally static so Blazor can diff renders of the same
            // instance. This fragment is built fresh once per HTTP response and discarded — there's
            // no repeat render to diff against, so a runtime-computed sequence is safe here.
#pragma warning disable ASP0006
            var sequence = 1;
            foreach (var (name, value) in parameters)
                builder.AddAttribute(sequence++, name, value);
#pragma warning restore ASP0006
            builder.CloseComponent();
        };

    internal static RazorComponentResult AggregateComponents(this RenderFragment[] fragments) =>
        new RazorComponentResult<FragmentContainer>(new Dictionary<string, object?>
        {
            [nameof(FragmentContainer.ChildContent)] = Combine(fragments)
        });

    private static RenderFragment Combine(RenderFragment[] fragments) => builder =>
    {
        foreach (var fragment in fragments)
            builder.AddContent(0, fragment);
    };
}
