using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Features.Feeds.Sources.Nyaa.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers <see cref="INyaaClient"/>. Relies entirely on the app-wide standard
        /// resilience handler (<c>AddWebAppDefaults</c>) for retry/circuit-breaker behavior —
        /// no custom pipeline. Binds <see cref="NyaaOptions"/> from configuration section
        /// "<see cref="NyaaOptions.SectionName"/>".
        /// </summary>
        public IHostApplicationBuilder AddNyaaClient()
        {
            builder.Services.Configure<NyaaOptions>(
                builder.Configuration.GetSection(NyaaOptions.SectionName));

            builder.Services.AddHttpClient<INyaaClient, NyaaClient>(static (sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<NyaaOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = opts.RequestTimeout;
            });

            return builder;
        }
    }
}
