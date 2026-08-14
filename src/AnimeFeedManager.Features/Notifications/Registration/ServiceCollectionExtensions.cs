using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Features.Notifications.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Binds <see cref="GmailOptions"/> from configuration section <see cref="GmailOptions.SectionName"/>.
        /// The sender itself isn't DI-registered — construct it as a delegate field via
        /// <see cref="GmailEmailSender.GmailEmailSenderHandler"/> where needed.
        /// </summary>
        public IHostApplicationBuilder AddGmailEmailSender()
        {
            builder.Services.Configure<GmailOptions>(
                builder.Configuration.GetSection(GmailOptions.SectionName));

            return builder;
        }
    }
}
