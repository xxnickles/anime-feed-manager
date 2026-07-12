using System.Net.ServerSentEvents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Infrastructure.Sse;

/// <summary>
/// Fluent builder of SSE event bindings. Each <c>Add</c>/<see cref="AddHtml{TEvent,TComponent}"/>
/// call binds one CLR event type to an <see cref="Audience"/> and a render step. The builder
/// produces an immutable list consumed by <see cref="SseStream"/>.
/// </summary>
public sealed class SseBindings
{
    private readonly List<SseBinding> _bindings = new();

    /// <summary>
    /// Bind <typeparamref name="TEvent"/> to <paramref name="eventName"/>. The
    /// <paramref name="render"/> function converts the event payload to a string
    /// (typically JSON). Same TEvent may be registered for different
    /// <paramref name="eventName"/>s if a feature wants multiple representations.
    /// </summary>
    public SseBindings Add<TEvent>(string eventName, Audience audience, Func<TEvent, string> render)
        where TEvent : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentNullException.ThrowIfNull(render);

        _bindings.Add(new SseBinding(
            EventType: typeof(TEvent),
            SseEventName: eventName,
            Audience: audience,
            Subscribe: (bus, writer, _) => bus.Subscribe<TEvent>(async (evt, _) =>
            {
                var rendered = render(evt);
                await writer.WriteAsync(new SseItem<string>(rendered, eventName));
            })));

        return this;
    }

    /// <summary>
    /// Bind <typeparamref name="TEvent"/>, rendering it through <typeparamref name="TComponent"/>
    /// via <see cref="HtmlRenderer"/>. The event is passed to the component as its <c>Model</c>
    /// parameter, matching the app's existing <c>RazorComponentResult&lt;TForm&gt;</c> rendering
    /// convention. A fresh <see cref="HtmlRenderer"/> is constructed and disposed per render, using
    /// the connection's own scoped <see cref="IServiceProvider"/> (see <see cref="SseStream"/>) —
    /// matching the framework's own non-request rendering samples, no artificial scope is created here.
    /// <para>
    /// Sent as an <c>event:</c>-less ("unnamed") SSE message so htmx 4's SSE extension auto-swaps it
    /// through the normal core pipeline rather than dispatching a DOM event nobody's listening for —
    /// naming this would silently stop it from ever reaching the DOM. <typeparamref name="TComponent"/>'s
    /// root markup is responsible for its own placement via <c>hx-swap-oob</c> (see the `htmx4` skill's
    /// SSE reference); this method only decides audience and rendering, never routing.
    /// </para>
    /// </summary>
    public SseBindings AddHtml<TEvent, TComponent>(Audience audience)
        where TEvent : notnull
        where TComponent : IComponent
    {
        _bindings.Add(new SseBinding(
            EventType: typeof(TEvent),
            SseEventName: typeof(TEvent).Name,
            Audience: audience,
            Subscribe: (bus, writer, serviceProvider) => bus.Subscribe<TEvent>(async (evt, _) =>
            {
                var rendered = await RenderHtml<TEvent, TComponent>(serviceProvider, evt);
                await writer.WriteAsync(new SseItem<string>(rendered));
            })));

        return this;
    }

    internal IReadOnlyList<SseBinding> Build() => _bindings.ToArray();

    private static async Task<string> RenderHtml<TEvent, TComponent>(IServiceProvider serviceProvider, TEvent model)
        where TComponent : IComponent
    {
        await using var htmlRenderer = new HtmlRenderer(
            serviceProvider,
            serviceProvider.GetRequiredService<ILoggerFactory>());

        return await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?> { ["Model"] = model });
            var output = await htmlRenderer.RenderComponentAsync<TComponent>(parameters);
            return output.ToHtmlString();
        });
    }
}
