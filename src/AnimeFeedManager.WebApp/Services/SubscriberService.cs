﻿using System.Collections.Immutable;
using System.Net.Http.Json;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.WebApp.Services;

public interface ISubscriberService
{
    Task<ImmutableList<string>> GetSubscriptions(string subscriber);
    Task<ImmutableList<string>> GetInterested(string subscriber);
    Task Subscribe(string subscriber, string series);
    Task Unsubscribe(string subscriber, string series);
    Task AddToInterest(string subscriber, string series);
    Task RemoveFromInterest(string subscriber, string series);
}

public class SubscriberService : ISubscriberService
{
    private readonly HttpClient _httpClient;

    public SubscriberService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImmutableList<string>> GetSubscriptions(string subscriber)
    {
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<string>>($"api/subscriptions/{subscriber}");
        return result.ToImmutableList();
    }

    public async Task<ImmutableList<string>> GetInterested(string subscriber)
    {
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<string>>($"api/interested/{subscriber}");
        return result.ToImmutableList();
    }

    public Task Subscribe(string subscriber, string series)
    {
        return _httpClient.PostAsJsonAsync("api/subscriptions", new SubscriptionDto(subscriber, series));
    }

    public Task AddToInterest(string subscriber, string series)
    {
        return _httpClient.PostAsJsonAsync("api/interested", new SubscriptionDto(subscriber, series));
    }

    public Task RemoveFromInterest(string subscriber, string series)
    {
        return _httpClient.PostAsJsonAsync("api/removeInterested", new SubscriptionDto(subscriber, series));
    }

    public Task Unsubscribe(string subscriber, string series)
    {
        return _httpClient.PostAsJsonAsync("api/unsubscribe", new SubscriptionDto(subscriber, series));
    }
}