using AnimeFeedManager.Web.Features.Common;
using AnimeFeedManager.Web.Features.Common.TvControls;

namespace AnimeFeedManager.Web.Features.Tv;

internal static class RenderFragments
{
    internal static Task<RenderedComponent> RenderSubscribedControls(this BlazorRenderer renderer, AvailableTvSeriesControlData data)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(SubscribedAnimeControls.ControlData), data}
        };

        return renderer.RenderComponent<SubscribedAnimeControls>(parameters);
    }
    
    internal static Task<RenderedComponent> RenderUnSubscribedControls(this BlazorRenderer renderer, AvailableTvSeriesControlData data)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(UnsubscribedAnimeControls.ControlData), data}
        };

        return renderer.RenderComponent<UnsubscribedAnimeControls>(parameters);
    }
    
    internal static Task<RenderedComponent> RenderInterestedControls(this BlazorRenderer renderer, NotAvailableControlData data)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(InterestedAnimeControls.ControlData), data}
        };

        return renderer.RenderComponent<InterestedAnimeControls>(parameters);
    }
    
    internal static Task<RenderedComponent> RenderNotAvailableControls(this BlazorRenderer renderer, NotAvailableControlData data)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(NotAvailableAnimeControls.ControlData), data}
        };

        return renderer.RenderComponent<NotAvailableAnimeControls>(parameters);
    }
}