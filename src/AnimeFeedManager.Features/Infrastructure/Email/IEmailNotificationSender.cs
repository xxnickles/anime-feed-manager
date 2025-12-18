using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Features.Infrastructure.Email;

/// <summary>
/// Delegate that creates a RenderFragment for an email template with a typed model
/// </summary>
/// <typeparam name="TModel">The model type used by the email template</typeparam>
/// <param name="model">The model data to render in the email</param>
/// <returns>A RenderFragment that can be rendered to HTML</returns>
public delegate RenderFragment EmailTemplateFactory<in TModel>(TModel model) where TModel : notnull;

/// <summary>
/// Service for sending emails using SMTP with Blazor component rendering
/// </summary>
public interface IEmailNotificationSender
{
    /// <summary>
    /// Sends an email using a Blazor component template
    /// </summary>
    /// <typeparam name="TModel">The model type for the email template</typeparam>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject line</param>
    /// <param name="templateFactory">Email template factory delegate that generates the content</param>
    /// <param name="model">The model data to pass to the template</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<Unit>> Send<TModel>(
        string to,
        string subject,
        EmailTemplateFactory<TModel> templateFactory,
        TModel model,
        CancellationToken cancellationToken = default
    ) where TModel : notnull;
}
