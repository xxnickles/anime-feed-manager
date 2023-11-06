using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.RealTime;

public class ClientNegotiation(ILoggerFactory loggerFactory)
{
    private readonly ILogger<ClientNegotiation> _logger = loggerFactory.CreateLogger<ClientNegotiation>();

    [Function("Negotiate")]
    public async Task<HttpResponseData> Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "negotiate")]
        HttpRequestData req,
        [SignalRConnectionInfoInput(HubName =  HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
        SignalRConnectionInfo  connectionInfo)
    {
        _logger.LogInformation("Creating signalr connection");
        return await req.Ok(connectionInfo);
    }
}