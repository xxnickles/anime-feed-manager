using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.RealTime;

public class ClientNegotiation
{
    private readonly ILogger<ClientNegotiation> _logger;

    public ClientNegotiation(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ClientNegotiation>();
    }

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