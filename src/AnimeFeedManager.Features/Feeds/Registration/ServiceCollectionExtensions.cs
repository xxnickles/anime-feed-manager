using AnimeFeedManager.Features.Feeds.Classification;
using AnimeFeedManager.Features.Feeds.Collection;
using AnimeFeedManager.Features.Feeds.Sources.AniList.Registration;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Registration;
using AnimeFeedManager.Features.Library.Events;
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
        /// <c>SeasonImported</c> event to build each series' platform set, the
        /// <see cref="TvReconciliationCronJob"/> TV reconciliation job, the
        /// <see cref="AiringClockCheckCronJob"/> cold-clock job for TV series unconfirmed on Nyaa,
        /// and the <see cref="NonAiringReconciliationCronJob"/> non-TV path. Depends on the host having
        /// already called <c>AddCosmosInfrastructure(...)</c>, <c>AddEventBus()</c>,
        /// <c>AddCronScheduler()</c>, and <c>AddLibrary()</c> (for <c>IJikanClient</c>).
        /// </summary>
        public IHostApplicationBuilder AddFeeds()
        {
            builder.AddNyaaClient();
            builder.AddAniListClient();

            builder.Services.AddEventHandler<SeasonImported, SeriesClassificationSubscriber>();

            builder.Services.AddScoped<TvReconciliationJob>();
            builder.Services.AddCronJob<TvReconciliationCronJob>();

            builder.Services.AddScoped<AiringClockCheckJob>();
            builder.Services.AddCronJob<AiringClockCheckCronJob>();

            builder.Services.AddScoped<NonAiringReconciliationJob>();
            builder.Services.AddCronJob<NonAiringReconciliationCronJob>();

            return builder;
        }
    }
}
