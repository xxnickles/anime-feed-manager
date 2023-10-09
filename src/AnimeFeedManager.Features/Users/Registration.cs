using AnimeFeedManager.Features.Users.IO;

namespace AnimeFeedManager.Features.Users;

public static class UsersRegistration
{
    public static IServiceCollection RegisterUserServices(this IServiceCollection services)
    {
        services.TryAddScoped<IUserStore, UserStore>();
        services.TryAddScoped<IUserGetter, UserGetter>();
        services.TryAddScoped<IUserEmailGetter, UserEmailGetter>();
        return services;
    }
}