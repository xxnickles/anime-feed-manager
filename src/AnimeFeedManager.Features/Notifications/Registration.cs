using AnimeFeedManager.Features.Notifications.IO;

namespace AnimeFeedManager.Features.Notifications;

public static class NotificationRegistration
{
    public static IServiceCollection RegisterNotificationServices(this IServiceCollection services)
    {
        services.TryAddScoped<IStoreNotification, StoreNotification>();
        return services;
    }
}