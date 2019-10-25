namespace AnimeFeedManager.Functions.Infrastructure
{
    public class SendGridConfiguration : ISendGridConfiguration
    {
        public string FromEmail { get; }
        public string FromName { get; }
        public bool Sandbox { get; }

        public SendGridConfiguration(string fromEmail, string fromName, bool sandBoxMode)
        {
            FromEmail = fromEmail;
            FromName = fromName;
            Sandbox = sandBoxMode;
        }
    }
}
