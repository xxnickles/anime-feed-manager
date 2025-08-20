using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Features.Images;

public static class Registration
{
    public static IServiceCollection RegisterImageServices(this IServiceCollection services)
    {
        services.AddHttpClient<IImageProvider, ImageProvider>(client =>
            {
                client.DefaultRequestHeaders.Add("user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
                client.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            }
        );
        return services;
    }
}