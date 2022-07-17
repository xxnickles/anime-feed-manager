using AnimeFeedManager.WebApp.Services;

namespace AnimeFeedManager.WebApp;

public static class ServiceRegistrationExtensions
{
    public static void RegisterHttpServices(this IServiceCollection Services, string baseApiUri)
    {
        Services.AddHttpClient<ISeasonFetcherService, SeasonService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp,_) => HttpClientPolicies.GetRetryPolicy(sp));

        Services.AddHttpClient<ISeasonCollectionFetcher, SeasonCollectionService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));

        Services.AddHttpClient<ISubscriberService, SubscriberService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));
        
        Services.AddHttpClient<IUserService, UserService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));
    }
}