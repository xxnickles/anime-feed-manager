using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnimeFeedManager.Features.Images;

public static class Registration
{
    public static IServiceCollection RegisterImageServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IImagesStore, ImagesStore>();
        return services;
    }
}