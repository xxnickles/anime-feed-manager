using AnimeFeedManager.Features.State.IO;

namespace AnimeFeedManager.Features.State;

public static class StateRegistration
{
    public static IServiceCollection RegisterStateServices(this IServiceCollection services)
    {
        services.TryAddScoped<ICreateState, CreateState>();
        services.TryAddScoped<IStateUpdater, StateUpdater>();
        return services;
    }
}