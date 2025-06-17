namespace AnimeFeedManager.Functions.Test;

public class QueueReceiverTest
{
    private readonly ILogger<QueueReceiverTest> _logger;

    public QueueReceiverTest(ILogger<QueueReceiverTest> logger)
    {
        _logger = logger;
    }

    [Function(nameof(QueueReceiverTest))]
    public void Run([QueueTrigger(SystemEvent.TargetQueue, Connection = Constants.AzureConnectionName)] SystemEvent message)
    {
       
        _logger.LogInformation("C# Queue trigger function processed: {Message}", message);
        
    }
}