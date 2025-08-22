using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;

namespace AnimeFeedManager.Functions.RealTime;

public class SignalRNegotiation
{
    private readonly ILogger _logger;

    public SignalRNegotiation(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SignalRNegotiation>();
    }

    [Function(nameof(SignalRNegotiation))]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, Route = "negotiate")]
        HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = HubNames.Notifications,
            ConnectionStringSetting = StorageRegistrationConstants.SignalRConnectionName)]
        SignalRConnectionInfo connectionInfo)
    {
        _logger.LogInformation("Creating signalr connection");
        return new OkObjectResult(connectionInfo);
    }
}