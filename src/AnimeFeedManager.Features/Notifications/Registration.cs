using AnimeFeedManager.Features.Notifications.IO;

namespace AnimeFeedManager.Features.Notifications;

public static class NotificationRegistration
{
    public static IServiceCollection RegisterNotificationServices(this IServiceCollection services)
    {
        services.TryAddScoped<IStoreNotification, StoreNotification>();
        services.TryAddScoped<IGetNotifications, GetNotifications>();
        services.TryAddScoped<FeedNotificationsCollector>();
        return services;
    }
}