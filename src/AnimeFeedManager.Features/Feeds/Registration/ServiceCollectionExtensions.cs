using AnimeFeedManager.Features.Feeds.Classification;
using AnimeFeedManager.Features.Feeds.Sources.AniList.Registration;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Features.Feeds.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers the Feeds feature: the Nyaa and AniList clients, and the
        /// <see cref="SeriesClassificationSubscriber"/> that reacts to Library's
        /// <c>SeasonImported</c> event to classify Trackable vs Untrackable series.
        /// Depends on the host having already called <c>AddCosmosInfrastructure(...)</c>,
        /// <c>AddEventBus()</c>, and <c>AddLibrary()</c> (for <c>IJikanClient</c>).
        /// </summary>
        public IHostApplicationBuilder AddFeeds()
        {
            builder.AddNyaaClient();
            builder.AddAniListClient();

            builder.Services.AddHostedService<SeriesClassificationSubscriber>();

            return builder;
        }
    }
}
