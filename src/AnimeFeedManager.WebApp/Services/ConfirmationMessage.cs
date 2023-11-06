using AnimeFeedManager.WebApp.Components.Common;
using MudBlazor;

namespace AnimeFeedManager.WebApp.Services;

public interface IConfirmationMessage
{
    Task<bool> GetConfirmation(string title, string message);
}

public class ConfirmationMessage(IDialogService dialogService) : IConfirmationMessage
{
    public async Task<bool> GetConfirmation(string title, string message)
    {
        var parameters = new DialogParameters {{nameof(ConfirmationDialog.Message), message}};
        var options = new DialogOptions {CloseOnEscapeKey = true};
        var dialog = await dialogService.ShowAsync<ConfirmationDialog>(title, parameters, options);
        var result = await dialog.Result;
        if (result.Canceled || result.Data == null) return false;
        return (bool) result.Data;
    }
}