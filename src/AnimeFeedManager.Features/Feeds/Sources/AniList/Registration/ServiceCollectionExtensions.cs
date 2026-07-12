using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Features.Feeds.Sources.AniList.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers <see cref="IAniListClient"/>. Relies entirely on the app-wide standard
        /// resilience handler (<c>AddWebAppDefaults</c>) for retry/circuit-breaker behavior —
        /// no custom pipeline. Binds <see cref="AniListOptions"/> from configuration section
        /// "<see cref="AniListOptions.SectionName"/>".
        /// </summary>
        public IHostApplicationBuilder AddAniListClient()
        {
            builder.Services.Configure<AniListOptions>(
                builder.Configuration.GetSection(AniListOptions.SectionName));

            builder.Services.AddHttpClient<IAniListClient, AniListClient>(static (sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<AniListOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = opts.RequestTimeout;
            });

            return builder;
        }
    }
}
