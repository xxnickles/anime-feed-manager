using AnimeFeedManager.Features.Users.IO;

namespace AnimeFeedManager.Features.Users;

public static class UsersRegistration
{
    public static IServiceCollection RegisterUserServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IUserStore, UserStore>();
        return services;
    }
}