
using AnimeFeedManager.Old.Features.Maintenance.IO;

namespace AnimeFeedManager.Old.Features.Maintenance;

public static class MaintenanceRegistration
{
    public static IServiceCollection RegisterMaintenanceServices(this IServiceCollection services)
    {
        services.TryAddScoped<IRemoveProcessedTitles, RemoveProcessedTitles>();
        services.TryAddScoped<IStorageCleanup, StorageCleanup>();
        return services;
    }
}