using System.Collections.Immutable;
using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services.Tv;

public interface ITvSubscriberService
{
    Task<ImmutableList<string>> GetSubscriptions(string subscriber, CancellationToken cancellationToken = default);
    Task<ImmutableList<string>> GetInterested(string subscriber, CancellationToken cancellationToken = default);
    Task Subscribe(string subscriber, string series, CancellationToken cancellationToken = default);
    Task Unsubscribe(string subscriber, string series, CancellationToken cancellationToken = default);
    Task AddToInterest(string subscriber, string series, CancellationToken cancellationToken = default);
    Task RemoveFromInterest(string subscriber, string series, CancellationToken cancellationToken = default);
}

public class TvSubscriberService : ITvSubscriberService
{
    private readonly HttpClient _httpClient;

    public TvSubscriberService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImmutableList<string>> GetSubscriptions(string subscriber, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/subscriptions/{subscriber}", cancellationToken);
        return await response.MapToListOfStrings();
    }

    public async Task<ImmutableList<string>> GetInterested(string subscriber, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/tv/interested/{subscriber}", cancellationToken);
        return await response.MapToListOfStrings();
    }

    public async Task Subscribe(string subscriber, string series, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/tv/subscriptions", new SimpleTvSubscription(subscriber, series), cancellationToken: cancellationToken);
        await response.CheckForProblemDetails();
    }

    public async Task AddToInterest(string subscriber, string series, CancellationToken cancellationToken = default)
    {
        var response= await _httpClient.PostAsJsonAsync("api/tv/interested", new SimpleTvSubscription(subscriber, series), cancellationToken: cancellationToken);
        await response.CheckForProblemDetails();
    }

    public async Task RemoveFromInterest(string subscriber, string series, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/tv/removeInterested", new SimpleTvSubscription(subscriber, series), cancellationToken: cancellationToken);
        await response.CheckForProblemDetails();
    }

    public  async Task Unsubscribe(string subscriber, string series, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/tv/unsubscribe", new SimpleTvSubscription(subscriber, series), cancellationToken: cancellationToken);
        await response.CheckForProblemDetails();
    }
}