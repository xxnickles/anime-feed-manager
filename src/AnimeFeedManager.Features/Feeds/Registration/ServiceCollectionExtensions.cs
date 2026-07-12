using AnimeFeedManager.Features.Feeds.Sources.AniList.Registration;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Registration;

namespace AnimeFeedManager.Features.Feeds.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddFeeds()
        {
            builder.AddNyaaClient();
            builder.AddAniListClient();

            return builder;
        }
    }
}
