using System.Text.Json;

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
    public Task Run([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {Now}", DateTime.Now);
        var payload = JsonSerializer.Serialize(new SeriesSeason(Season.Spring(), Year.FromNumber(2020)),
            SeriesSeasonContext.Default.SeriesSeason);
        return _domainPostman.SendMessage(
                new SystemEvent(
                    TargetConsumer.Admin(),
                    EventTarget.Both,
                    EventType.Information,
                    new EventPayload(payload, nameof(SeriesSeason))
                ))
            .Match(_ => _logger.LogInformation("Message sent"),
                e => e.LogError(_logger));
    }
}