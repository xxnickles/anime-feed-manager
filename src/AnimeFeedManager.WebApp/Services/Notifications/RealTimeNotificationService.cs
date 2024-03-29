﻿using System.Collections.Immutable;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using Blazored.LocalStorage;

namespace AnimeFeedManager.WebApp.Services.Notifications;

public interface IRealtimeNotificationService
{
    event Action? NotificationsUpdated;
    ImmutableList<ServerNotification> Notifications { get; }
    Task AddNotification(ServerNotification notification);
    Task LoadLocalNotifications();
    Task SetNotificationViewed(string id);
    Task SetAllNotificationViewed();
    Task RemoveAll();
    Task RemoveAdminNotifications();
}

public class RealTimeRealtimeNotificationService(ILocalStorageService localStorage) : IRealtimeNotificationService
{
    private const string NotificationsKey = "notifications";
    private const byte MaxNotifications = 5;

    public event Action? NotificationsUpdated;

    public ImmutableList<ServerNotification> Notifications { private set; get; } =
        ImmutableList<ServerNotification>.Empty;

    public async Task AddNotification(ServerNotification notification)
    {
        if (Notifications.Count >= MaxNotifications)
        {
            Notifications = Notifications.RemoveRange(0, Notifications.Count - MaxNotifications + 1);
        }

        Notifications = Notifications.Add(notification);
        await localStorage.SetItemAsync(NotificationsKey, Notifications);
        NotificationsUpdated?.Invoke();
    }


    public async Task LoadLocalNotifications()
    {
        if (await localStorage.ContainKeyAsync(NotificationsKey))
        {
            var storedNotifications =
                await localStorage.GetItemAsync<IEnumerable<ServerNotification>>(NotificationsKey);
            Notifications = storedNotifications?.ToImmutableList() ?? ImmutableList<ServerNotification>.Empty;
        }
        else
        {
            Notifications = ImmutableList<ServerNotification>.Empty;
        }

        NotificationsUpdated?.Invoke();
    }

    public async Task SetNotificationViewed(string id)
    {
        var target = Notifications.FirstOrDefault(n => n.Id == id);
        if (target is not null)
        {
            Notifications = Notifications.Replace(target, target with { Read = true });
            await localStorage.SetItemAsync(NotificationsKey, Notifications);
            NotificationsUpdated?.Invoke();
        }
    }

    public async Task SetAllNotificationViewed()
    {
        Notifications = Notifications.ConvertAll(n => n with { Read = true });
        await localStorage.SetItemAsync(NotificationsKey, Notifications);
        NotificationsUpdated?.Invoke();
    }

    public async Task RemoveAll()
    {
        Notifications = ImmutableList<ServerNotification>.Empty;
        await localStorage.RemoveItemAsync(NotificationsKey);
        NotificationsUpdated?.Invoke();
    }

    public async Task RemoveAdminNotifications()
    {
        if (Notifications.Any())
        {
            Notifications = Notifications.Where(n => n.Audience != TargetAudience.Admins).ToImmutableList();
            await localStorage.SetItemAsync(NotificationsKey, Notifications);
            NotificationsUpdated?.Invoke();
        }
    }
}