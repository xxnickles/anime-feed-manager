namespace AnimeFeedManager.Functions.Test;

public class TestPostman
{
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<TestPostman> _logger;

    public TestPostman(
        IDomainPostman domainPostman,
        ILogger<TestPostman> logger)
    {
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function("TestPostman")]
    public Task Run([TimerTrigger("*/15 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {Now}", DateTime.Now);

        return _domainPostman.SendMessage(new TestMessage(Season.Spring, Year.FromNumber(2025)))
            .Match(_ => _logger.LogInformation("Message sent"), 
                e => e.LogError(_logger));
    }
}