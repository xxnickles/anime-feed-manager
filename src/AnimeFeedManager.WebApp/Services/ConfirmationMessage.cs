using AnimeFeedManager.WebApp.Components.Common;
using MudBlazor;

namespace AnimeFeedManager.WebApp.Services;

public interface IConfirmationMessage
{
    Task<bool> GetConfirmation(string title, string message);
}

public class ConfirmationMessage : IConfirmationMessage
{
    private readonly IDialogService _dialogService;

    public ConfirmationMessage(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }
    
    public async Task<bool> GetConfirmation(string title, string message)
    {
        var parameters = new DialogParameters {{nameof(ConfirmationDialog.Message), message}};
        var options = new DialogOptions {CloseOnEscapeKey = true};
        var dialog = await _dialogService.ShowAsync<ConfirmationDialog>(title, parameters, options);
        var result = await dialog.Result;
        if (result.Cancelled || result.Data == null) return false;
        return (bool) result.Data;
    }
}