using System.Collections.Immutable;
using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface ISubscriberService
{
    Task<ImmutableList<string>> GetSubscriptions(string subscriber, CancellationToken cancellationToken = default);
    Task<ImmutableList<string>> GetInterested(string subscriber, CancellationToken cancellationToken = default);
    Task Subscribe(string subscriber, string series, CancellationToken cancellationToken = default);
    Task Unsubscribe(string subscriber, string series, CancellationToken cancellationToken = default);
    Task AddToInterest(string subscriber, string series, CancellationToken cancellationToken = default);
    Task RemoveFromInterest(string subscriber, string series, CancellationToken cancellationToken = default);
}

public class SubscriberService : ISubscriberService
{
    private readonly HttpClient _httpClient;

    public SubscriberService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImmutableList<string>> GetSubscriptions(string subscriber, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/subscriptions/{subscriber}", cancellationToken);
        return await response.MapToList<string>();
    }

    public async Task<ImmutableList<string>> GetInterested(string subscriber, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/interested/{subscriber}", cancellationToken);
        return await response.MapToList<string>();
    }

    public Task Subscribe(string subscriber, string series, CancellationToken cancellationToken = default)
    {
        return _httpClient.PostAsJsonAsync("api/subscriptions", new SubscriptionDto(subscriber, series), cancellationToken: cancellationToken);
    }

    public Task AddToInterest(string subscriber, string series, CancellationToken cancellationToken = default)
    {
        return _httpClient.PostAsJsonAsync("api/interested", new SubscriptionDto(subscriber, series), cancellationToken: cancellationToken);
    }

    public Task RemoveFromInterest(string subscriber, string series, CancellationToken cancellationToken = default)
    {
        return _httpClient.PostAsJsonAsync("api/removeInterested", new SubscriptionDto(subscriber, series), cancellationToken: cancellationToken);
    }

    public Task Unsubscribe(string subscriber, string series, CancellationToken cancellationToken = default)
    {
        return _httpClient.PostAsJsonAsync("api/unsubscribe", new SubscriptionDto(subscriber, series), cancellationToken: cancellationToken);
    }
}