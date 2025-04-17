using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Types;
using AnimeFeedManager.Web.Features.Tv.Controls;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Tv;

internal static class ComponentResponses
{
    internal static RazorComponentResult OkResponse<T>(AvailableTvSeriesControlData data, string message)
        where T : AvailableSeriesBase
    {
        var parameters = new Dictionary<string, object?>
        {
            { nameof(AvailableSeriesBase.ControlData), data },
            { nameof(AvailableSeriesBase.Message), message }
        };
        return new RazorComponentResult<T>(parameters);
    }

    internal static RazorComponentResult ErrorResponse<T>(AvailableTvSeriesControlData data, DomainError domainError,
        ILogger logger) where T : AvailableSeriesBase
    {
        domainError.LogError(logger);
        var parameters = new Dictionary<string, object?>
        {
            { nameof(AvailableSeriesBase.ControlData), data },
            { nameof(AvailableSeriesBase.DomainError), domainError }
        };
        return new RazorComponentResult<T>(parameters);
    }

    internal static RazorComponentResult OkResponse<T>(NotAvailableControlData data, string message)
        where T : NotAvailableSeriesBase
    {
        var parameters = new Dictionary<string, object?>
        {
            { nameof(NotAvailableSeriesBase.ControlData), data },
            { nameof(NotAvailableSeriesBase.Message), message }
        };
        return new RazorComponentResult<T>(parameters);
    }

    internal static RazorComponentResult ErrorResponse<T>(NotAvailableControlData data, DomainError domainError,
        ILogger logger) where T : NotAvailableSeriesBase
    {
        domainError.LogError(logger);
        var parameters = new Dictionary<string, object?>
        {
            { nameof(NotAvailableSeriesBase.ControlData), data },
            { nameof(NotAvailableSeriesBase.DomainError), domainError }
        };
        return new RazorComponentResult<T>(parameters);
    }

    internal static RazorComponentResult OkResponse(SeasonInformation seasonInformation, string message)
    {
        var parameters = new Dictionary<string, object?>
        {
            { nameof(TvGridComponent.SeasonInfo), seasonInformation },
            { nameof(TvGridComponent.Message), message }
        };
        return new RazorComponentResult<TvGridComponent>(parameters);
    }
}