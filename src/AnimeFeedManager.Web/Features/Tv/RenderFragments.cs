using AnimeFeedManager.Web.Features.Common;
using AnimeFeedManager.Web.Features.Common.TvControls;

namespace AnimeFeedManager.Web.Features.Tv;

internal static class RenderFragments
{
    internal static Task<RenderedComponent> RenderSubscribedControls(BlazorRenderer renderer, AvailableTvSeriesControlData data)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(SubscribedAnimeControls.ControlData), data}
        };

        return renderer.RenderComponent<SubscribedAnimeControls>(parameters);
    }
    
    internal static Task<RenderedComponent> RenderUnSubscribedControls(BlazorRenderer renderer, AvailableTvSeriesControlData data)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(UnsubscribedAnimeControls.ControlData), data}
        };

        return renderer.RenderComponent<UnsubscribedAnimeControls>(parameters);
    }
    
    internal static Task<RenderedComponent> RenderInterestedControls(BlazorRenderer renderer, NotAvailableControlData data)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(InterestedAnimeControls.ControlData), data}
        };

        return renderer.RenderComponent<InterestedAnimeControls>(parameters);
    }
    
    internal static Task<RenderedComponent> RenderNotAvailableControls(BlazorRenderer renderer, NotAvailableControlData data)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(NotAvailableAnimeControls.ControlData), data}
        };

        return renderer.RenderComponent<NotAvailableAnimeControls>(parameters);
    }
}