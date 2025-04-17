using AnimeFeedManager.Old.Features.Users.IO;

namespace AnimeFeedManager.Old.Features.Users;

public static class UsersRegistration
{
    public static IServiceCollection RegisterUserServices(this IServiceCollection services)
    {
        services.TryAddScoped<IUserRoleGetter, UserRoleGetter>();
        services.TryAddScoped<IUserStore, UserStore>();
        services.TryAddScoped<IUserGetter, UserGetter>();
        services.TryAddScoped<IUserRoleGetter, UserRoleGetter>();
        services.TryAddScoped<IUserEmailGetter, UserEmailGetter>();
        services.TryAddScoped<IUserVerification, UserVerification>();
        services.TryAddScoped<CleanAllSubscriptions>();
        services.TryAddScoped<CopyAllSubscriptions>();
        services.TryAddScoped<SubscriptionCopierSetter>();

        return services;
    }

    public static IServiceCollection RegisterPasswordlessServices(this IServiceCollection services)
    {
        services.TryAddScoped<IUserDelete, UserDelete>();
        services.TryAddScoped<IPasswordlessRegistration, PasswordlessRegistration>();
        services.TryAddScoped<IPasswordlessLogin, PasswordlessLogin>();
        return services;
    }
}