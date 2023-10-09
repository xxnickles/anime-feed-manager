
using AnimeFeedManager.Features.Maintenance.IO;

namespace AnimeFeedManager.Features.Maintenance
{
    public static class MaintenanceRegistration
    {
        public static IServiceCollection RegisterMaintenanceServices(this IServiceCollection services)
        {
            services.TryAddScoped<IRemoveProcessedTitles, RemoveProcessedTitles>();
            services.TryAddScoped<IStorageCleanup, StorageCleanup>();
            return services;
        }
    }
}