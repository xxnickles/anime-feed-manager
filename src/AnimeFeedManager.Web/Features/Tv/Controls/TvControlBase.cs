using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Tv.Controls;

public abstract class AvailableSeriesBase : ComponentBase
{
    [Parameter, EditorRequired] public AvailableTvSeriesControlData? ControlData { get; set; }
    [Parameter] public string Message { get; set; } = string.Empty;
    [Parameter] public DomainError? DomainError { get; set; }
}

public abstract class NotAvailableSeriesBase : ComponentBase
{
    [Parameter, EditorRequired] public NotAvailableControlData? ControlData { get; set; }
    [Parameter] public string Message { get; set; } = string.Empty;
    [Parameter] public DomainError? DomainError { get; set; }
}