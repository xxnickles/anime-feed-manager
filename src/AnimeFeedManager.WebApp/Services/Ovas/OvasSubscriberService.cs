using System.Collections.Immutable;
using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services.Ovas;

public interface IOvasSubscriberService
{
    Task<ImmutableList<string>> GetSubscriptions(string subscriber, CancellationToken cancellationToken = default);

    Task Subscribe(string subscriber, string series, DateTime notificationDate,
        CancellationToken cancellationToken = default);

    Task Unsubscribe(string subscriber, string series, CancellationToken cancellationToken = default);
}

public class OvasSubscriberService : IOvasSubscriberService
{
    private readonly HttpClient _httpClient;

    public OvasSubscriberService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImmutableList<string>> GetSubscriptions(string subscriber,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/ovas/subscriptions/{subscriber}", cancellationToken);
        return await response.MapToListOfStrings();
    }

    public async Task Subscribe(string subscriber, string series, DateTime notificationDate,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ovas/subscriptions",
            new ShortSeriesSubscription(subscriber, series, notificationDate),
            ShortSeriesSubscriptionContext.Default.ShortSeriesSubscription, cancellationToken: cancellationToken);
        await response.CheckForProblemDetails();
    }

    public async Task Unsubscribe(string subscriber, string series, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ovas/subscriptions/unsubscribe",
            new ShortSeriesUnsubscribe(subscriber, series),
            ShortSeriesUnsubscribeContext.Default.ShortSeriesUnsubscribe,
            cancellationToken: cancellationToken);
        await response.CheckForProblemDetails();
    }
}