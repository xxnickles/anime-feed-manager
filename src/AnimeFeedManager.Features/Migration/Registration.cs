using AnimeFeedManager.Features.Migration.IO;

namespace AnimeFeedManager.Features.Migration;

public static class MigrationRegistration
{
    public static IServiceCollection RegisterMigration(this IServiceCollection services)
    {
        services.TryAddScoped<SeriesMigration>();
        services.TryAddScoped<UserMigration>();
        return services;
    }
}