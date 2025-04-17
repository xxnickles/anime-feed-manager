using AnimeFeedManager.Old.Features.State.IO;

namespace AnimeFeedManager.Old.Features.State;

public static class StateRegistration
{
    public static IServiceCollection RegisterStateServices(this IServiceCollection services)
    {
        services.TryAddScoped<IStateCreator, StateCreator>();
        services.TryAddScoped<IStateUpdater, StateUpdater>();
        return services;
    }
}