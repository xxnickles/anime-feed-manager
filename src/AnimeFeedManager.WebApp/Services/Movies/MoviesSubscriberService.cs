using System.Collections.Immutable;
using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services.Movies;

public interface IMoviesSubscriberService
{
    Task<ImmutableList<string>> GetSubscriptions(string subscriber, CancellationToken cancellationToken = default);

    Task Subscribe(string subscriber, string series, DateTime notificationDate,
        CancellationToken cancellationToken = default);

    Task Unsubscribe(string subscriber, string series, CancellationToken cancellationToken = default);
}

public class MoviesSubscriberService : IMoviesSubscriberService
{
    private readonly HttpClient _httpClient;

    public MoviesSubscriberService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImmutableList<string>> GetSubscriptions(string subscriber,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Movies/subscriptions/{subscriber}", cancellationToken);
        return await response.MapToListOfStrings();
    }

    public async Task Subscribe(string subscriber, string series, DateTime notificationDate,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Movies/subscriptions",
            new ShortSeriesSubscription(subscriber, series, notificationDate), cancellationToken: cancellationToken);
        await response.CheckForProblemDetails();
    }

    public async Task Unsubscribe(string subscriber, string series, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Movies/subscriptions/unsubscribe",
            new ShortSeriesUnsubscribe(subscriber, series), cancellationToken: cancellationToken);
        await response.CheckForProblemDetails();
    }
}