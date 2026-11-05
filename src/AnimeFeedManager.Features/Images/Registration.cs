using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Features.Images;

public static class Registration
{
    public static IServiceCollection RegisterImageServices(this IServiceCollection services)
    {
        services.AddHttpClient<IImageProvider, ImageProvider>(client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("AnimeFeedManager/1.0");
                client.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            }
        )
        // AutomaticDecompression owns the Accept-Encoding header. Setting that header by hand
        // instead leaves the handler unaware, and CDNs that honour it answer with gzip bytes
        // that then get stored verbatim as the image.
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        });
        return services;
    }
}