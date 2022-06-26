namespace AnimeFeedManager.Functions.Infrastructure;

public interface ISendGridConfiguration
{
    string FromEmail { get; }
    string FromName { get; }
    bool Sandbox { get; }
}