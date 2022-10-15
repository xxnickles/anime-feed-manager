using AnimeFeedManager.WebApp.Services;

namespace AnimeFeedManager.WebApp;

public static class ServiceRegistrationExtensions
{
    public static void RegisterHttpServices(this IServiceCollection services, string baseApiUri)
    {
        services.AddHttpClient<ISeasonFetcherService, SeasonService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp,_) => HttpClientPolicies.GetRetryPolicy(sp));

        services.AddHttpClient<ITvCollectionFetcher, TvCollectionService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));

        services.AddHttpClient<ITvSubscriberService, TvSubscriberService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));
        
        services.AddHttpClient<IUserService, UserService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));

        services.AddHttpClient<IAdminService, AdminService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));
        
        services.AddHttpClient<IOvasCollectionService, OvasCollectionService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));
        
        services.AddHttpClient<IServerNotificationProcessingService, ServerNotificationProcessingService>(client =>
            client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));

        services.AddScoped<INotificationService,NotificationService>();
    }
}