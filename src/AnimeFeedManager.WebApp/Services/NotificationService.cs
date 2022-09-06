using System.Collections.Immutable;
using Blazored.LocalStorage;

namespace AnimeFeedManager.WebApp.Services;

public interface INotificationService
{
    event Action? NotificationsUpdated;
    ImmutableList<ServeNotification> Notifications { get; }
    Task AddNotification(ServeNotification notification);
    Task LoadLocalNotifications();
    Task SetNotificationViewed(string id);
    Task SetAllNotificationViewed();
}

public class NotificationService : INotificationService
{
    private readonly ILocalStorageService _localStorage;
    private const string NotificationsKey = "notifications";

    public NotificationService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public event Action? NotificationsUpdated;

    public ImmutableList<ServeNotification> Notifications { private set; get; } =
        ImmutableList<ServeNotification>.Empty;

    public async Task AddNotification(ServeNotification notification)
    {
        if (Notifications.Count >= 5)
        {
            Notifications = Notifications.RemoveRange(0, Notifications.Count - 10);
        }

        Notifications = Notifications.Add(notification);
        await _localStorage.SetItemAsync(NotificationsKey, Notifications);
        NotificationsUpdated?.Invoke();
    }


    public async Task LoadLocalNotifications()
    {
        var storedNotifications = await _localStorage.GetItemAsync<IEnumerable<ServeNotification>>(NotificationsKey);
        Notifications = storedNotifications?.ToImmutableList() ?? ImmutableList<ServeNotification>.Empty;
        NotificationsUpdated?.Invoke();
    }

    public async Task SetNotificationViewed(string id)
    {
        var target = Notifications.FirstOrDefault(n => n.Id == id);
        if (target is not null)
        {
            Notifications = Notifications.Replace(target, target with { Read = true });
            await _localStorage.SetItemAsync(NotificationsKey, Notifications);
        }
    }

    public async Task SetAllNotificationViewed()
    {
        Notifications = Notifications.ConvertAll(n => n with { Read = true });
        await _localStorage.SetItemAsync(NotificationsKey, Notifications);
        NotificationsUpdated?.Invoke();
    }
}