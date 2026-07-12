using AnimeFeedManager.Features.Feeds.Classification;
using AnimeFeedManager.Features.Feeds.Collection;
using AnimeFeedManager.Features.Feeds.Sources.AniList.Registration;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Registration;
using AnimeFeedManager.Infrastructure.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Features.Feeds.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers the Feeds feature: the Nyaa and AniList clients, the
        /// <see cref="SeriesClassificationSubscriber"/> that reacts to Library's
        /// <c>SeasonImported</c> event to classify Trackable vs Untrackable series, the
        /// <see cref="NyaaCollectionCronJob"/> hot-path collection job, and the
        /// <see cref="AiringClockCheckCronJob"/> cold-clock job for Untrackable series.
        /// Depends on the host having already called <c>AddCosmosInfrastructure(...)</c>,
        /// <c>AddEventBus()</c>, <c>AddCronScheduler()</c>, and <c>AddLibrary()</c>
        /// (for <c>IJikanClient</c>).
        /// </summary>
        public IHostApplicationBuilder AddFeeds()
        {
            builder.AddNyaaClient();
            builder.AddAniListClient();

            builder.Services.AddHostedService<SeriesClassificationSubscriber>();

            builder.Services.AddScoped<NyaaCollectionJob>();
            builder.Services.AddCronJob<NyaaCollectionCronJob>();

            builder.Services.AddScoped<AiringClockCheckJob>();
            builder.Services.AddCronJob<AiringClockCheckCronJob>();

            return builder;
        }
    }
}
