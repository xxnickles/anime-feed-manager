using Microsoft.Azure.SignalR.Management;

namespace AnimeFeedManager.Web.Features.RealTime;

internal static class RealTimeRegistration
{
    private const string ConnectionName = "signalr";

    internal static void RegisterRealTimeServices(this WebApplicationBuilder builder)
    {
        var serviceManager = new ServiceManagerBuilder()
            .WithOptions(options =>
            {
                options.ConnectionString = builder.Configuration.GetConnectionString(ConnectionName);
                options.ServiceTransportType = ServiceTransportType.Transient;
            })
            .BuildServiceManager();

        builder.Services.AddSingleton(new NotificationsHub(serviceManager));
    }
}
