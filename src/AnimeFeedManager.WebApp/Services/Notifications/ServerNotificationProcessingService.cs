using System.Diagnostics;
using System.Net.Http.Json;
using AnimeFeedManager.Common.Domain.Notifications;
using AnimeFeedManager.Common.RealTimeNotifications;
using AnimeFeedManager.WebApp.State;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnimeFeedManager.WebApp.Services.Notifications;

public interface IServerNotificationProcessingService
{
    event Func<SeasonProcessNotification, Task>? SeasonProcessNotification;
    event Func<ImageUpdateNotification, Task>? ImagesUpdateNotification;
    event Func<TitlesUpdateNotification, Task>? TitlesUpdateNotification;
    event Func<HubConnectionStatus, ValueTask>? ConnectionStatus;
    public event Action<Exception>? ExceptionRisen;

    Task AddToGroup();
    Task RemoveFromGroup();
    Task SubscribeToNotifications();
}

public class ServerNotificationProcessingService(
    ApplicationState state,
    SeasonSideEffects seasonSideEffects,
    HttpClient httpClient,
    ILogger<ServerNotificationProcessingService> logger)
    : IServerNotificationProcessingService
{
    private enum DelayedActions
    {
        AddUser,
        RemoveUser
    }

    private HubConnection? _hubConnection;

    public event Func<ImageUpdateNotification, Task>? ImagesUpdateNotification;
    public event Func<TitlesUpdateNotification, Task>? TitlesUpdateNotification;
    public event Func<HubConnectionStatus, ValueTask>? ConnectionStatus;
    public event Action<Exception>? ExceptionRisen;
    public event Func<SeasonProcessNotification, Task>? SeasonProcessNotification;

    private readonly HashSet<DelayedActions> _delayedActions = new();

    private const string AddToGroupEndpoint = "api/notifications/setup";
    private const string RemoveFromGroupEndpoint = "api/notifications/remove";

    public async Task SubscribeToNotifications()
    {
        ConnectionStatus += OnConnectionStatus;

        try
        {
            var response = await httpClient.PostAsync("api/negotiate", new StringContent(string.Empty));
            var info = await response.Content.ReadFromJsonAsync(ConnectionInfoContext.Default.ConnectionInfo);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(info?.Url ?? string.Empty,
                    options => { options.AccessTokenProvider = () => Task.FromResult(info?.AccessToken); })
                .WithAutomaticReconnect()
                .Build();

            SubscribeToHubStatus(_hubConnection);
            SubscribeToNotifications(_hubConnection);
            await _hubConnection.StartAsync();
            logger.LogInformation("Connection to notification hub has been completed successfully");
            await RiseStatusChange(HubConnectionStatus.Connected);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GHub connection bootstrapping failed");
            ExceptionRisen?.Invoke(ex);
        }
    }

    public async Task AddToGroup()
    {
        if (IsConnectedToHub())
        {
            await ExecuteAction(AddToGroupEndpoint);
        }
        else
        {
            _delayedActions.Add(DelayedActions.AddUser);
        }
    }

    public async Task RemoveFromGroup()
    {
        if (IsConnectedToHub())
        {
            await ExecuteAction(RemoveFromGroupEndpoint);
        }
        else
        {
            _delayedActions.Add(DelayedActions.RemoveUser);
        }
    }

    private bool IsConnectedToHub() => _hubConnection?.State is HubConnectionState.Connected &&
                                       !string.IsNullOrEmpty(_hubConnection.ConnectionId);

    private void SubscribeToNotifications(HubConnection hubConnection)
    {
        hubConnection.On<SeasonProcessNotification>(ServerNotifications.SeasonProcess, async notification =>
        {
            SeasonProcessNotification?.Invoke(notification);
            if (!state.Value.AvailableSeasons.Contains(notification.SimpleSeason))
            {
                await seasonSideEffects.LoadAvailableSeasons(state, true);
            }
        });

        hubConnection.On<TitlesUpdateNotification>(ServerNotifications.TitleUpdate,
            notification => { TitlesUpdateNotification?.Invoke(notification); });

        hubConnection.On<ImageUpdateNotification>(ServerNotifications.ImageUpdate,
            notification => { ImagesUpdateNotification?.Invoke(notification); });
    }

    private void SubscribeToHubStatus(HubConnection hubConnection)
    {
        hubConnection.Closed += ConnectionLost;
        hubConnection.Reconnected += ConnectionRecovered;
    }

    private async Task ConnectionRecovered(string? arg)
    {
        logger.LogInformation("Connection to the hub has recovered {Id}", arg);
        await RiseStatusChange(HubConnectionStatus.Connected);
    }

    private async Task ConnectionLost(Exception? arg)
    {
        logger.LogError(arg, "Connection to the hub has been lost");
        await RiseStatusChange(HubConnectionStatus.Disconnected);
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
                            await ExecuteAction(AddToGroupEndpoint);
                            break;
                        case DelayedActions.RemoveUser:
                            await ExecuteAction(RemoveFromGroupEndpoint);
                            break;
                        default:
                            throw new UnreachableException("Invalid Action Value");
                    }

                    _delayedActions.Remove(action);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "An error has occurred when executing delayed {Action}", action);
                }
            }
        }
    }

    private async Task ExecuteAction(string endpoint)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(endpoint,
                new HubInfo(_hubConnection?.ConnectionId ?? string.Empty), HubInfoContext.Default.HubInfo);
            await response.CheckForProblemDetails();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Group action execution failed");
            ExceptionRisen?.Invoke(ex);
        }
    }

    private async Task RiseStatusChange(HubConnectionStatus status)
    {
        if (ConnectionStatus != null)
            await ConnectionStatus.Invoke(status);
    }
}