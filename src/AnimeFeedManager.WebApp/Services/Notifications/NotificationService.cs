using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services.Notifications;
public interface INotificationService
{
    Task<UiNotifications> GetNotifications(string user, CancellationToken cancellationToken = default);
}

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;

    public NotificationService( HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<UiNotifications> GetNotifications(string user, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/notifications/{user}", cancellationToken);
        return await response.MapToObject<UiNotifications>(new EmptyUINotifications());
    }
}