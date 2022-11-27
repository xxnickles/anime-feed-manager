namespace AnimeFeedManager.WebApp.Components.Admin;

public record UpdateData(
    string Title,
    string Description,
    string ConfirmationTitle,
    string ConfirmationMessage,
    string UpdateMessage,
    string ErrorContext);

public record DefaultUpdateData() : UpdateData(string.Empty,string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);