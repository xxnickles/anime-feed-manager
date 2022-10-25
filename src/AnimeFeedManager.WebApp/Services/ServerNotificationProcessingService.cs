using System.Net.Http.Json;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.WebApp.State;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnimeFeedManager.WebApp.Services;

public interface IServerNotificationProcessingService
{
    event Func<SeasonProcessNotification, Task>? SeasonProcessNotification;
    event Func<TitlesUpdateNotification, Task>? TitlesUpdateNotification;
    event Action<HubConnectionStatus>? ConnectionStatus;
    Task AddToGroup();
    Task RemoveFromGroup();
    Task SubscribeToNotifications();
}

public class ServerNotificationProcessingService : IServerNotificationProcessingService
{
    private HubConnection? _hubConnection;

    private readonly ApplicationState _state;
    private readonly SeasonSideEffects _seasonSideEffects;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ServerNotificationProcessingService> _logger;
    
    public event Func<TitlesUpdateNotification, Task>? TitlesUpdateNotification;
    public event Action<HubConnectionStatus>? ConnectionStatus;

    public event Func<SeasonProcessNotification, Task>? SeasonProcessNotification;

    public ServerNotificationProcessingService(
        ApplicationState state,
        SeasonSideEffects seasonSideEffects,
        HttpClient httpClient,
        ILogger<ServerNotificationProcessingService> logger)
    {
        _state = state;
        _seasonSideEffects = seasonSideEffects;
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

    public async Task AddToGroup()
    {
        if (!IsConnectedToHub()) throw new HubException("Hub connection is not available at this time");
        
        var response = await _httpClient.PostAsJsonAsync("api/notifications/setup",
            new HubInfo(_hubConnection?.ConnectionId ?? string.Empty));
        await response.CheckForProblemDetails();
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
        hubConnection.On<SeasonProcessNotification>(ServerNotifications.SeasonProcess,
            notification =>
            {
                SeasonProcessNotification?.Invoke(notification);
                if (!_state.Value.AvailableSeasons.Contains(notification.Season))
                {
                    _seasonSideEffects.LoadAvailableSeasons(_state, true);
                }
            });
        
        hubConnection.On<TitlesUpdateNotification>(ServerNotifications.TitleUpdate,
            notification =>
            {
                TitlesUpdateNotification?.Invoke(notification);
            });
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