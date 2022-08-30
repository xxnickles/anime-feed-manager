using System.Net.Http.Json;
using AnimeFeedManager.Common.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnimeFeedManager.WebApp.Services;

public enum HubConnectionStatus
{
    None,
    Connected,
    Disconnected,
}

public interface INotificationService
{
    event Action<string>? NewNotification;
    event Action<SeasonProcessNotification>? SeasonProcessNotification;
    event Action<HubConnectionStatus>? ConnectionStatus;
    Task AddToGroup();
    Task RemoveFromGroup();
    Task SubscribeToNotifications();
}

public class NotificationService : INotificationService
{
    private HubConnection? _hubConnection;
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationService> _logger;

    public event Action<string>? NewNotification;
    public event Action<HubConnectionStatus>? ConnectionStatus;

    public event Action<SeasonProcessNotification>? SeasonProcessNotification;

    public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SubscribeToNotifications()
    {
        var response = await _httpClient.PostAsync("api/negotiate", new StringContent(string.Empty));
        var info = await response.Content.ReadFromJsonAsync<ConnectionInfo>();

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(info?.Url ?? string.Empty,
                options => { options.AccessTokenProvider = () => Task.FromResult(info?.AccessToken); })
            .WithAutomaticReconnect()
            .Build();

        SubscribeToHubStatus(_hubConnection);
        SubscribeToNotifications(_hubConnection);
        await _hubConnection.StartAsync();
        _logger.LogInformation("Connection to notification hub has been completed successfully");
        ConnectionStatus?.Invoke(HubConnectionStatus.Connected);
    }

    public Task AddToGroup()
    {
        if (IsConnectedToHub())
        {
            return _httpClient.PostAsJsonAsync("api/notifications/setup",
                new HubInfo(_hubConnection?.ConnectionId ?? string.Empty));
        }

        throw new HubException("Hub connection is not available at this time");
    }

    private bool IsConnectedToHub() => _hubConnection?.State is HubConnectionState.Connected &&
                                       !string.IsNullOrEmpty(_hubConnection.ConnectionId);

    public Task RemoveFromGroup()
    {
        if (IsConnectedToHub())
        {
            return _httpClient.PostAsJsonAsync("api/notifications/remove",
                new HubInfo(_hubConnection?.ConnectionId ?? string.Empty));
        }

        throw new HubException("Hub connection is not available at this time");
    }

    private void SubscribeToNotifications(HubConnection hubConnection)
    {
        hubConnection.On<string>(ServerNotifications.TestNotification,
            notification => { NewNotification?.Invoke(notification); });

        hubConnection.On<SeasonProcessNotification>(ServerNotifications.SeasonProcess,
            notification => { SeasonProcessNotification?.Invoke(notification); });
    }

    private void SubscribeToHubStatus(HubConnection hubConnection)
    {
        hubConnection.Closed += ConnectionLost;
        hubConnection.Reconnected += ConnectionRecovered;
    }

    private Task ConnectionRecovered(string? arg)
    {
        _logger.LogInformation("Connection to the hub has recovered {Id}", arg);
        ConnectionStatus?.Invoke(HubConnectionStatus.Connected);
        return Task.CompletedTask;
    }

    private Task ConnectionLost(Exception? arg)
    {
        _logger.LogError(arg, "Connection to the hub has been lost");
        ConnectionStatus?.Invoke(HubConnectionStatus.Disconnected);
        return Task.CompletedTask;
    }
}