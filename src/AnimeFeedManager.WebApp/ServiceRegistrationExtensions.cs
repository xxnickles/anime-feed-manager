using AnimeFeedManager.WebApp.Services;

namespace AnimeFeedManager.WebApp;

public static class ServiceRegistrationExtensions
{
    public static void RegisterHttpServices(this IServiceCollection services, string baseApiUri)
    {
        services.AddHttpClient<ISeasonFetcherService, SeasonService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp,_) => HttpClientPolicies.GetRetryPolicy(sp));

        services.AddHttpClient<ISeasonCollectionFetcher, SeasonCollectionService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));

        services.AddHttpClient<ISubscriberService, SubscriberService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));
        
        services.AddHttpClient<IUserService, UserService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));

        services.AddHttpClient<IAdminService, AdminService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));
    }
}