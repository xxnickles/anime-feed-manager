using AnimeFeedManager.Features.State.IO;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Features.State;

public static class StateRegistration
{
    public static IServiceCollection RegisterStateServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateState, CreateState>();
        services.AddScoped<IUpdateState, UpdateState>();
        return services;
    }
}