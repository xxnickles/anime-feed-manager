using AnimeFeedManager.Features.Images.IO;

namespace AnimeFeedManager.Features.Images;

public static class ImageRegistration
{
    public static IServiceCollection RegisterImageServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IImagesBlobStore, AzureImagesBlobStore>();
        services.TryAddScoped<ITvImageStorage, TvImageStorage>();
        services.TryAddScoped<IOvasImageStorage, OvasImageStorage>();
        services.TryAddScoped<IMoviesImageStorage, MoviesImageStorage>();
        services.TryAddScoped<ImageAdder>();
        services.TryAddScoped<ScrapImagesNotificationHandler>();
        return services;
    }
}