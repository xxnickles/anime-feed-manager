using AnimeFeedManager.Features.Tv.Library.Events;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

internal static class TvSeriesCompleted
{
    internal static (string, RenderFragment) ForTvSeriesCompleted(CompletedTvSeriesResult notification) => (
        GetTitle(notification),
        builder =>
        {
            // Body
            builder.AddContent(0, GetBody(notification));
        });


    private static string GetBody(CompletedTvSeriesResult result)
    {
        return result.ResultType switch
        {
            ResultType.Failed => "Completion process for TV Series has failed after feed updates.",
            ResultType.Success => $"{result.Titles.Length} series have been completed successfully.",
            _ => throw new ArgumentOutOfRangeException()
            
        };
    } 
    
    private static string GetTitle(CompletedTvSeriesResult result)
    {
        return result.ResultType switch
        {
            ResultType.Failed => "Completing TV Series process has failed.",
            ResultType.Success => "Completing TV Series process has been completed successfully.",
            _ => throw new ArgumentOutOfRangeException()
            
        };
    }
    
}