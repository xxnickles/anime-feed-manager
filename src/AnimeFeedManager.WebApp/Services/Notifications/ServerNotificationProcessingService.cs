using System.Diagnostics;
using System.Net.Http.Json;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.WebApp.State;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnimeFeedManager.WebApp.Services.Notifications;

public interface IServerNotificationProcessingService
{
    event Func<SeasonProcessNotification, Task>? SeasonProcessNotification;
    event Func<TitlesUpdateNotification, Task>? TitlesUpdateNotification;
    event Func<HubConnectionStatus, ValueTask>? ConnectionStatus;
    Task AddToGroup();
    Task RemoveFromGroup();
    Task SubscribeToNotifications();
}

public class ServerNotificationProcessingService : IServerNotificationProcessingService, IDisposable
{
    private enum DelayedActions
    {
        AddUser,
        RemoveUser
    }

    private HubConnection? _hubConnection;

    private readonly ApplicationState _state;
    private readonly SeasonSideEffects _seasonSideEffects;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ServerNotificationProcessingService> _logger;

    public event Func<TitlesUpdateNotification, Task>? TitlesUpdateNotification;
    public event Func<HubConnectionStatus, ValueTask>? ConnectionStatus;
    public event Func<SeasonProcessNotification, Task>? SeasonProcessNotification;

    private readonly HashSet<DelayedActions> _delayedActions = new();

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
        ConnectionStatus += OnConnectionStatus;
        
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
        if (IsConnectedToHub())
        {
            await AddUserToGroup();
        }
        else
        {
            _delayedActions.Add(DelayedActions.AddUser);
        }
    }

    private async Task AddUserToGroup()
    {
        var response = await _httpClient.PostAsJsonAsync("api/notifications/setup",
            new HubInfo(_hubConnection?.ConnectionId ?? string.Empty));
        await response.CheckForProblemDetails();
    }

    private bool IsConnectedToHub() => _hubConnection?.State is HubConnectionState.Connected &&
                                       !string.IsNullOrEmpty(_hubConnection.ConnectionId);

    public async Task RemoveFromGroup()
    {
        if (IsConnectedToHub())
        {
            await RemoveUserFromGroup();

        }
        else
        {
            _delayedActions.Add(DelayedActions.RemoveUser);
        }
    }

    public async Task RemoveUserFromGroup()
    {
        var response = await _httpClient.PostAsJsonAsync("api/notifications/remove",
                new HubInfo(_hubConnection?.ConnectionId ?? string.Empty));
        await response.CheckForProblemDetails();
    }



    private void SubscribeToNotifications(HubConnection hubConnection)
    {
        hubConnection.On<SeasonProcessNotification>(ServerNotifications.SeasonProcess, async notification =>
        {
            SeasonProcessNotification?.Invoke(notification);
            if (!_state.Value.AvailableSeasons.Contains(notification.Season))
            {
                await _seasonSideEffects.LoadAvailableSeasons(_state, true);
            }
        });

        hubConnection.On<TitlesUpdateNotification>(ServerNotifications.TitleUpdate,
            notification => { TitlesUpdateNotification?.Invoke(notification); });
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

    private async ValueTask OnConnectionStatus(HubConnectionStatus status)
    {
        if (status == HubConnectionStatus.Connected && _delayedActions.Any())
        {
            foreach (var action in _delayedActions)
            {
                try
                {
                    switch (action)
                    {
                        case DelayedActions.AddUser:
                            await AddUserToGroup();
                            break;
                        case DelayedActions.RemoveUser:
                            await RemoveUserFromGroup();
                            break;
                        default:
                            throw new UnreachableException("Invalid Action Value");
                    }

                    _delayedActions.Remove(action);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error has occurred when executing delayed {Action}", action);
                }
            }
        }
    }

    public void Dispose()
    {
        ConnectionStatus -= OnConnectionStatus;
    }
}