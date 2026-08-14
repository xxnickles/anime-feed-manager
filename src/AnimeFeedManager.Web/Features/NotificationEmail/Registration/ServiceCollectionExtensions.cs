using Microsoft.AspNetCore.Components.Web;

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
    }
}
