using AnimeFeedManager.Features.Notifications;
using AnimeFeedManager.Features.Notifications.Registration;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Web.Features.NotificationEmail.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers <see cref="HtmlRenderer"/> scoped — its component-rendering dependencies are
        /// already satisfied by <c>AddRazorComponents</c>. The renderer delegate itself isn't
        /// DI-registered; construct it via <see cref="BlazorEmailRenderer.NotificationEmailRendererHandler"/>
        /// where needed.
        /// </summary>
        public IHostApplicationBuilder AddNotificationEmailRenderer()
        {
            builder.Services.AddScoped<HtmlRenderer>();
            return builder;
        }

        /// <summary>
        /// Composition root for the notification-dispatch job: wires the Gmail sender config and the
        /// Blazor renderer, registers <see cref="NotificationEmailRenderer"/> scoped (built from the
        /// scoped <see cref="HtmlRenderer"/>) so <see cref="NotificationDispatchCronJob"/> — which
        /// lives in <c>Features</c> and only knows the delegate, never <see cref="HtmlRenderer"/> —
        /// resolves normally via plain constructor injection.
        /// </summary>
        public IHostApplicationBuilder AddNotificationDispatch()
        {
            builder.AddNotificationEmailRenderer();
            builder.AddGmailEmailSender();

            builder.Services.AddScoped<NotificationEmailRenderer>(sp =>
                sp.GetRequiredService<HtmlRenderer>().NotificationEmailRendererHandler());
            builder.Services.AddScoped<NotificationDispatchCronJob>();

            return builder;
        }
    }
}
