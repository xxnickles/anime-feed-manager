namespace AnimeFeedManager.Functions.Infrastructure;

public class SendGridConfiguration : ISendGridConfiguration
{
    public SendGridConfiguration(string fromEmail, string fromName, bool sandBoxMode)
    {
        FromEmail = fromEmail;
        FromName = fromName;
        Sandbox = sandBoxMode;
    }

    public string FromEmail { get; }
    public string FromName { get; }
    public bool Sandbox { get; }
}