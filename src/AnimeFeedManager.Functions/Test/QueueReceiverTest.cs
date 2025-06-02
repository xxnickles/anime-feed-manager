namespace AnimeFeedManager.Functions.Test;

public class QueueReceiverTest
{
    private readonly ILogger<QueueReceiverTest> _logger;

    public QueueReceiverTest(ILogger<QueueReceiverTest> logger)
    {
        _logger = logger;
    }

    [Function(nameof(QueueReceiverTest))]
    public void Run([QueueTrigger(TestMessage.TargetQueue, Connection = Constants.AzureConnectionName)] TestMessage message)
    {
       
        _logger.LogInformation("C# Queue trigger function processed: {Message}", message);
        
    }
}