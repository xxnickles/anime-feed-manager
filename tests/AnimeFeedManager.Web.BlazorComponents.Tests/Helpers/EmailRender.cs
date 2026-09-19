using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Web.BlazorComponents.Tests.Helpers;

/// <summary>
/// Renders the notification email through the same path the sender uses.
/// </summary>
internal static partial class EmailRender
{
    public static async Task<string> Html(NotificationModel model)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        await using var provider = services.BuildServiceProvider();
        await using var htmlRenderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        return await new BlazorRenderer(htmlRenderer).RenderComponent<WrapperComponent>(
            new Dictionary<string, object?>
            {
                [nameof(WrapperComponent.ChildContent)] = NotificationEmail.AsRenderFragment(model)
            });
    }

    /// <summary>
    /// Episode numbers in the order the rendered part lists them. Both parts label
    /// episodes as "EP &lt;number&gt;", so one pattern reads either; the number runs
    /// to the next space or, in the HTML part, the tag that closes the badge.
    /// </summary>
    public static string[] EpisodeNumbersInOrder(string rendered) =>
        EpisodeLabel().Matches(rendered).Select(match => match.Groups[1].Value).ToArray();

    [GeneratedRegex(@"EP\s+([^\s<]+)")]
    private static partial Regex EpisodeLabel();
}
