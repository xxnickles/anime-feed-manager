using AnimeFeedManager.Storage.Interface;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Maintenance;

public class CleanStorage
{
    private readonly IStorageCleanup _storageCleanup;
    private readonly ILogger<CleanStorage> _logger;

    public CleanStorage(IStorageCleanup storageCleanup, ILoggerFactory loggerFactory)
    {
        _storageCleanup = storageCleanup;
        _logger = loggerFactory.CreateLogger<CleanStorage>();
    }

    [Function("CleanOldNotifications")]
    public async Task RunCleanNotifications([TimerTrigger("0 0 0 1 * *")] TimerInfo timer)
    {
        var result = await _storageCleanup.CleanOldNotifications();
        result.Match(
            _ => { _logger.LogInformation("Old notification has been cleaned"); },
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
    
    [Function("CleanOldState")]
    public async Task RunCleanState([TimerTrigger("0 0 10 * * SAT")] TimerInfo timer)
    {
        var result = await _storageCleanup.CleanOldState();
        result.Match(
            _ => { _logger.LogInformation("Old state has been cleaned"); },
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}