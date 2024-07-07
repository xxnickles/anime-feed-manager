using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.RealTime;

public class SignalRNegotiation
{
    private readonly ILogger _logger;

    public SignalRNegotiation(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SignalRNegotiation>();
    }

    [Function(nameof(SignalRNegotiation))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = HubNames.Notifications,
            ConnectionStringSetting = "SignalRConnectionString")]
        SignalRConnectionInfo connectionInfo)
    {
        _logger.LogInformation("Creating signalr connection");
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(connectionInfo);
        return response;
    }
}