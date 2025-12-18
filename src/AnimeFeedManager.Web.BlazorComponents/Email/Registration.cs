using AnimeFeedManager.Features.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnimeFeedManager.Web.BlazorComponents.Email;

public static class Registration
{
    public static void RegisterEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GmailOptions>(configuration.GetSection(GmailOptions.SectionName));
        services.TryAddScoped<IEmailNotificationSender, EmailNotificationSender>();
    }
}
