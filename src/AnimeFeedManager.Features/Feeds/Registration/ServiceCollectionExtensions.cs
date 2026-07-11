namespace AnimeFeedManager.Features.Feeds.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddFeeds() => builder;
    }
}
