using AnimeFeedManager.Features.Users.IO;

namespace AnimeFeedManager.Features.Users;

public static class UsersRegistration
{
    public static IServiceCollection RegisterUserServices(this IServiceCollection services)
    {
        services.TryAddScoped<IUserRoleGetter, UserRoleGetter>();
        services.TryAddScoped<IUserStore, UserStore>();
        services.TryAddScoped<IUserGetter, UserGetter>();
        services.TryAddScoped<IUserRoleGetter, UserRoleGetter>();
        services.TryAddScoped<IUserEmailGetter, UserEmailGetter>();
        services.TryAddScoped<IUserDelete, UserDelete>();
        services.TryAddScoped<IPasswordlessRegistration, PasswordlessRegistration>();
        services.TryAddScoped<IPasswordlessLogin, PasswordlessLogin>();
        services.TryAddScoped<IUserVerification, UserVerification>();
        services.TryAddScoped<CleanAllSubscriptions>();
        services.TryAddScoped<CopyAllSubscriptions>();
        services.TryAddScoped<SubscriptionCopierSetter>();

        return services;
    }
}